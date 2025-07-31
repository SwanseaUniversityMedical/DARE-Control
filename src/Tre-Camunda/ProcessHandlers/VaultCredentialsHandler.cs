using System.Diagnostics;
using System.Text.Json;
using Zeebe.Client.Accelerator.Attributes;
using Zeebe.Client.Accelerator.Abstractions;
using BL.Services;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("store-in-vault")]
    public class VaultCredentialsHandler : IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
    {
        private readonly VaultCredentialsService _vaultCredentialsService;
        private readonly ILogger<VaultCredentialsHandler> _logger;

        public VaultCredentialsHandler(VaultCredentialsService vaultCredentialsService, ILogger<VaultCredentialsHandler> logger)
        {
            _vaultCredentialsService = vaultCredentialsService;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellation)
        {
            var SW = new Stopwatch();
            SW.Start();

            _logger.LogDebug($"StoreInVaultHandler started for process instance {job.ProcessInstanceKey}");

            try
            {
               
                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);

               
                var vaultPath = variables["vaultPath"]?.ToString(); //Might need to change this based on current logic
                var credentialDataJson = variables["credentialData"]?.ToString();

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

               
                Dictionary<string, object> credentialData;
                try
                {
                    credentialData = JsonSerializer.Deserialize<Dictionary<string, object>>(credentialDataJson);
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
    
    }
}
