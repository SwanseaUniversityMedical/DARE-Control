
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using Tre_Camunda.Models;
using Tre_Camunda.Services;
using Tre_Credentials.DbContexts;
using Tre_Credentials.Models;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("create-postgres-user")]
    public class CreatePostgresUserHandler : IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
    {
        private readonly IPostgreSQLUserManagementService _postgreSQLUserManagementService;
        private readonly IVaultCredentialsService _vaultCredentialsService;
        private readonly ILogger<CreatePostgresUserHandler> _logger;
        private readonly CredentialsDbContext _credentialsDbContext;

        public CreatePostgresUserHandler(IPostgreSQLUserManagementService postgresSQLUserManagementService, ILogger<CreatePostgresUserHandler> logger, IVaultCredentialsService vaultCredentialsService, CredentialsDbContext credentialsDbContext)
        {
            _postgreSQLUserManagementService = postgresSQLUserManagementService;
            _logger = logger;
            _vaultCredentialsService = vaultCredentialsService;
            _credentialsDbContext = credentialsDbContext;
        }

        public async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("CreatePostgresUserHandler started. processInstance={ProcessInstanceKey}", job.ProcessInstanceKey);

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
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "postgres", "No credential information found in envList");
                    return CreateStatusResponse("ERROR: Missing credentials, cannot proceed.");
                }
                else
                {

                    string? username = envList.Where(x => x.env.ToLower().Contains("username")).FirstOrDefault().value.ToString();
                    string? database = envList.Where(x => x.env.ToLower().Contains("database")).FirstOrDefault().value.ToString();
                    string? server = envList.Where(x => x.env.ToLower().Contains("server")).FirstOrDefault().value.ToString();
                    string? port = envList.Where(x => x.env.ToLower().Contains("port")).FirstOrDefault().value.ToString();
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

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(project)) 
                    {
                        await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "postgres",
                            "Missing credentials; cannot proceed with Postgres user creation.");
                        return CreateStatusResponse("ERROR: Missing credentials, cannot proceed.");
                    }

                    var password = GenerateSecurePassword();

                    var schemaPermissions = new List<SchemaPermission>
                {
                    new SchemaPermission
                    {
                        SchemaName = project,
                        Permissions = DatabasePermissions.Read | DatabasePermissions.Write | DatabasePermissions.CreateTables
                    }
                };

                    var createUserRequest = new CreateUserRequest
                    {
                        Username = username,
                        Password = password,
                        Server = server,
                        Datasbasename = database,
                        Port = port,
                        SchemaPermissions = schemaPermissions
                    };

                    var result = await _postgreSQLUserManagementService.CreateUserAsync(createUserRequest);
                    if (!result.Success)
                    {
                        await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "postgres",
                            $"Failed to create PostgreSQL user: {result.ErrorMessage}");

                        return CreateStatusResponse("ERROR: Failed credential creation");
                    }

                    var credentialData = new Dictionary<string, object>();
                    foreach (var credential in envList)
                    {
                        var CredentialEnv = new CredentialsVault();
                        CredentialEnv.env = credential.env;
                        if (credential.env.ToLower().Contains("password"))
                        {
                            CredentialEnv.value = password;
                        }
                        else
                        {
                            CredentialEnv.value = credential.value;
                        }

                        credentialData.Add(CredentialEnv.env, CredentialEnv.value);

                    }

                    var jobId = submissionId;
                    string vaultPath = $"postgres/{user}/{jobId}/{project}";
                    var vaultResult = await _vaultCredentialsService.AddCredentialAsync(vaultPath, credentialData);
                    if (!vaultResult)
                    {
                        await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "postgres",
                            $"Failed to store credential in Vault at path: {vaultPath}");
                        return CreateStatusResponse("ERROR: Credential store in vault failed");
                    }


                    await CreateCredentialsReadyMessageAsync(submissionId, parentProcessKey, processInstanceKey, vaultPath);

                    _logger.LogInformation("Successfully created PostgreSQL user: {Username} for project: {Project}", username, project);
                    return CreateStatusResponse($"OK: PostgreSQL user '{username}' created for project '{project}'.");
                }
            }
            catch (OperationCanceledException)
            {

                throw;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unexpected error in CreatePostgresUserHandler. processInstance={ProcessInstanceKey}", processInstanceKey);

                await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "postgres",
                    $"Unexpected error: {ex.Message}");

                return CreateStatusResponse("Unexpected Error in Postgres handler");
            }
            finally
            {
                if (sw.IsRunning) sw.Stop();
                _logger.LogInformation("CreatePostgresUserHandler took {Seconds} seconds", sw.Elapsed.TotalSeconds);
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
                    CredentialType = "postgres",
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
    }
}
