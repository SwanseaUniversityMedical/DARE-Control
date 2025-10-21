using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Tre_Camunda.Models;
using Tre_Camunda.Services;
using Tre_Credentials.DbContexts;
using Tre_Credentials.Models;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("create-trino-user")]
    public class CreateTrinoUserHandler : IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
    {
        private readonly ILogger<CreateTrinoUserHandler> _logger;
        private readonly ILdapUserManagementService _ldapUserManagementService;
        private readonly IVaultCredentialsService _vaultCredentialsService;
        private readonly CredentialsDbContext _credentialsDbContext;

        public CreateTrinoUserHandler(
            ILogger<CreateTrinoUserHandler> logger,
            ILdapUserManagementService ldapUserManagementService,
            IVaultCredentialsService vaultCredentialsService,
            CredentialsDbContext credentialsDbContext)
        {
            _logger = logger;
            _ldapUserManagementService = ldapUserManagementService;
            _vaultCredentialsService = vaultCredentialsService;
            _credentialsDbContext = credentialsDbContext;
        }

        public async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("CreateTrinoUserHandler started. processInstance={ProcessInstanceKey}", job.ProcessInstanceKey);

            string? submissionId = null;
            long? parentProcessKey = null;
            long processInstanceKey = job.ProcessInstanceKey;

            try
            {
                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);
                var envListJson = variables["envList"]?.ToString();
                var envList = JsonSerializer.Deserialize<List<CredentialsCamundaOutput>>(envListJson);

                if (envList?.FirstOrDefault() == null)
                {
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "trino",
                        "No credential information found in envList");
                    return CreateStatusResponse("ERROR: Missing credentials, cannot proceed.");
                }

                string? username = envList.Where(x => x.env.ToLower().Contains("username")).FirstOrDefault()?.value?.ToString();
                string? project = variables["project"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "");
                string? user = variables["user"]?.ToString().Replace("[", "").Replace("]", "");

                var subItem = envList.FirstOrDefault(x => string.Equals(x.env, "submissionId", StringComparison.OrdinalIgnoreCase));
                submissionId = subItem?.value;

                if (variables.TryGetValue("parentProcessKey", out var parentObj))
                {
                    if (parentObj is JsonElement el && el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out var parsed))
                        parentProcessKey = parsed;
                    else if (long.TryParse(parentObj?.ToString(), out var parsed2))
                        parentProcessKey = parsed2;
                }

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(project) || string.IsNullOrEmpty(user))
                {
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "trino",
                        "Missing credentials; cannot proceed with Trino user creation.");
                    return CreateStatusResponse("ERROR: Missing credentials, cannot proceed.");
                }

                var password = GenerateSecurePassword();

                var createUserRequest = new CreateUserRequest
                {
                    Username = username,
                    Password = password,
                    CanLogin = true,
                    CanCreateDb = false,
                    CanCreateRole = false
                };

                var result = await _ldapUserManagementService.CreateUserAsync(createUserRequest);

                if (!result.Success)
                {
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "trino",
                        $"Failed to create Trino user: {result.ErrorMessage}");
                    return CreateStatusResponse("ERROR: Failed credential creation");
                }

                var credentialData = new Dictionary<string, object>();
                foreach (var credential in envList)
                {
                    var credentialEnv = new CredentialsVault();
                    credentialEnv.env = credential.env;

                    if (credential.env.ToLower().Contains("password"))
                    {
                        credentialEnv.value = password;
                    }
                    else
                    {
                        credentialEnv.value = credential.value;
                    }

                    credentialData.Add(credentialEnv.env, credentialEnv.value);
                }

                var jobId = submissionId;
                var userId = CleanDnValue(user);
                string vaultPath = $"trino/{userId}/{jobId}/{project}";

                var vaultResult = await _vaultCredentialsService.AddCredentialAsync(vaultPath, credentialData);
                if (!vaultResult)
                {
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "trino",
                        $"Failed to store credential in Vault at path: {vaultPath}");
                    return CreateStatusResponse("ERROR: Credential store in vault failed");
                }

                await CreateCredentialsReadyMessageAsync(submissionId, parentProcessKey, processInstanceKey, vaultPath);

                _logger.LogInformation("Successfully created Trino user: {Username} for project: {Project}", username, project);
                return CreateStatusResponse($"OK: Trino user '{username}' created for project '{project}'.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateTrinoUserHandler. processInstance={ProcessInstanceKey}", processInstanceKey);

                await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "trino",
                    $"Unexpected error: {ex.Message}");

                return CreateStatusResponse("Unexpected Error in Trino handler");
            }
            finally
            {
                if (sw.IsRunning) sw.Stop();
                _logger.LogInformation("CreateTrinoUserHandler took {Seconds} seconds", sw.Elapsed.TotalSeconds);
            }
        }

        private async Task RecordErrorAsync(string submissionId, long? parentProcessKey, long processInstanceKey, string credentialType, string errorMessage)
        {
            try
            {
                var submission = int.Parse(submissionId);
                var existing = await _credentialsDbContext.EphemeralCredentials.FirstOrDefaultAsync(x => x.SubmissionId == submission && x.ProcessInstanceKey == processInstanceKey);

                if (existing != null)
                {
                    _logger.LogWarning("EphemeralCredential already exists for SubmissionId={SubmissionId} and ProcessKey={ProcessInstanceKey}", submissionId, processInstanceKey);
                    return;
                }

                var row = new EphemeralCredential
                {
                    SubmissionId = submission,
                    ParentProcessInstanceKey = parentProcessKey,
                    ProcessInstanceKey = processInstanceKey,
                    CreatedAt = DateTime.UtcNow,
                    IsProcessed = false,
                    CredentialType = credentialType,
                    VaultPath = null,
                    SuccessStatus = SuccessStatus.Error,
                    ErrorMessage = errorMessage
                };

                _credentialsDbContext.EphemeralCredentials.Add(row);
                await _credentialsDbContext.SaveChangesAsync();

                _logger.LogInformation("Recorded error for processInstance={ProcessInstanceKey}: {Message}", processInstanceKey, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record error. processInstance={ProcessInstanceKey}", processInstanceKey);
            }
        }

        private async Task CreateCredentialsReadyMessageAsync(string submissionId, long? parentProcessKey, long processInstanceKey, string vaultPath)
        {
            try
            {
                var submissionGuid = int.Parse(submissionId);
                var existing = await _credentialsDbContext.EphemeralCredentials
                    .FirstOrDefaultAsync(x => x.SubmissionId == submissionGuid && x.ProcessInstanceKey == processInstanceKey);

                if (existing != null)
                {
                    _logger.LogWarning("EphemeralCredential already exists for SubmissionId={SubmissionId}, ProcessInstanceKey={ProcessInstanceKey}",
                        submissionId, processInstanceKey);
                    return;
                }

                var credReadyMessage = new EphemeralCredential
                {
                    SubmissionId = submissionGuid,
                    ParentProcessInstanceKey = parentProcessKey,
                    ProcessInstanceKey = processInstanceKey,
                    CreatedAt = DateTime.UtcNow,
                    IsProcessed = false,
                    VaultPath = vaultPath,
                    CredentialType = "trino",
                    SuccessStatus = SuccessStatus.Success
                };

                _credentialsDbContext.EphemeralCredentials.Add(credReadyMessage);
                await _credentialsDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating credentials ready message for submission: {SubmissionId}", submissionId);
            }
        }

        private Dictionary<string, object> CreateStatusResponse(string text)
        {
            return new Dictionary<string, object>
            {
                ["statusText"] = text
            };
        }

        private string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string CleanDnValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var charsToRemove = new[] { '[', ']', '\\', '"' };
            var sb = new StringBuilder();
            foreach (var c in value)
            {
                if (!charsToRemove.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}