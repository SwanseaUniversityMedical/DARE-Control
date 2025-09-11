using System.Diagnostics;
using System.Text.Json;
using Zeebe.Client.Accelerator.Attributes;
using Zeebe.Client.Accelerator.Abstractions;
using Tre_Camunda.Services;
using Tre_Credentials.DbContexts;
using Hangfire;
using Tre_Credentials.Models;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("store-in-vault")]
    public class VaultCredentialsHandler : IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
    {
        private readonly IVaultCredentialsService _vaultCredentialsService;
        private readonly ILogger<VaultCredentialsHandler> _logger;
        private readonly CredentialsDbContext _credentialsDbContext;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public VaultCredentialsHandler(IVaultCredentialsService vaultCredentialsService, ILogger<VaultCredentialsHandler> logger, CredentialsDbContext credentialsDbContext, IBackgroundJobClient backgroundJobClient)
        {
            _vaultCredentialsService = vaultCredentialsService;
            _logger = logger;
            _credentialsDbContext = credentialsDbContext;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellation)
        {
            var SW = new Stopwatch();
            SW.Start();

            _logger.LogDebug($"StoreInVaultHandler started for process instance {job.ProcessInstanceKey}");

            try
            {

                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);


                var vaultPath = variables["vaultPath"]?.ToString();
                var credentialDataJson = variables["credentialData"]?.ToString();

                var submissionId = variables["submissionId"]?.ToString();
                var processInstanceKey = job.ProcessInstanceKey;


                if (string.IsNullOrEmpty(vaultPath))
                {
                    var errorMsg = "Missing vaultPath for Vault storage";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                if (string.IsNullOrEmpty(credentialDataJson))
                {
                    var errorMsg = "Missing credentialData for Vault storage";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                if (string.IsNullOrEmpty(submissionId))
                {
                    var errorMsg = "Missing submissionId for Vault storage";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                Dictionary<string, object> credentialData;
                try
                {
                    var rawData = JsonSerializer.Deserialize<Dictionary<string, object>>(credentialDataJson);


                    credentialData = new Dictionary<string, object>();
                    foreach (var x in rawData)
                    {
                        if (x.Value is JsonElement element)
                        {
                            credentialData[x.Key] = element.ValueKind switch
                            {
                                JsonValueKind.String => element.GetString(),
                                JsonValueKind.Number => element.TryGetInt64(out var longVal) ? longVal : element.GetDouble(),
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                JsonValueKind.Null => null,
                                _ => element.GetRawText()
                            };
                        }
                        else
                        {
                            credentialData[x.Key] = x.Value;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    var errorMsg = $"Invalid credentialData JSON format: {ex.Message}";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                if (credentialData == null || credentialData.Count == 0)
                {
                    var errorMsg = "credentialData is empty or null";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }


                var success = await _vaultCredentialsService.AddCredentialAsync(vaultPath, credentialData);

                if (!success)
                {
                    var errorMsg = $"Failed to store credential in Vault at path: {vaultPath}";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                await CreateCredentialsReadyMessage(submissionId, processInstanceKey, vaultPath);

                var outputVariables = new Dictionary<string, object>
                {
                    ["vaultPath"] = vaultPath,
                    ["vaultStorageStatus"] = "success",
                    ["storedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),

                    /* Pass addiitonal variables if needed by next job, otherwise not needed */
                    ["credentialType"] = variables.ContainsKey("credentialType") ? variables["credentialType"] : "unknown",
                    ["project"] = variables.ContainsKey("project") ? variables["project"] : null,
                    ["userId"] = variables.ContainsKey("userId") ? variables["userId"] : null
                };

                _logger.LogInformation($"Successfully stored credential in Vault at path: {vaultPath}");

                SW.Stop();
                _logger.LogInformation($"StoreInVaultHandler took {SW.Elapsed.TotalSeconds} seconds");

                return outputVariables;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error in StoreInVaultHandler: {ex.Message}";
                _logger.LogError(ex, errorMsg);

                SW.Stop();
                _logger.LogInformation($"StoreInVaultHandler took {SW.Elapsed.TotalSeconds} seconds");

                throw;
            }
        }

        private async Task CreateCredentialsReadyMessage(string submissionId, long processInstanceKey, string vaultPath)
        {
            try
            {
                var submissionGuid = Guid.Parse(submissionId);

                var credReadyMessage = new EphemeralCredsReadyMessage
                {
                    SubmissionId = submissionGuid,
                    ProcessInstanceKey = processInstanceKey,
                    CreatedAt = DateTime.UtcNow,
                    IsProcessed = false,
                    VaultPath = vaultPath
                };

                _credentialsDbContext.EphemeralCredsReadyMessages.Add(credReadyMessage);

                await _credentialsDbContext.SaveChangesAsync();
                
                //This method immediately fires a notification when the creds are ready, may not be needed as recurring job is set in TRE-API
                BackgroundJob.Enqueue(() => LogCredentialsReady(submissionGuid, processInstanceKey, vaultPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating credentials ready message for submission: {submissionId}");
            }

        }

        public Task LogCredentialsReady(Guid submissionId, long processInstanceKey, string vaultPath)
        {
            _logger.LogInformation($"Credentials ready for Submission: {submissionId} and ProcessInstance: {processInstanceKey} and VaultPath: {vaultPath}");
            return Task.CompletedTask;
        }
    }   
}
