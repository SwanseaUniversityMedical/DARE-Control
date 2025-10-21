using Tre_Camunda.Services;
using Tre_Camunda.Models;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Accelerator.Attributes;
using Zeebe.Client.Accelerator.Abstractions;
using System.Diagnostics;
using System.Text.Json;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("delete-trino-user")]
    public class DeleteTrinoUserHandler : IAsyncZeebeWorker
    {
        private readonly ILogger<DeleteTrinoUserHandler> _logger;
        private readonly ILdapUserManagementService _ldapUserManagementService;
        private readonly IVaultCredentialsService _vaultCredentialsService;
        private readonly IEphemeralCredentialsService _ephemeralCredentialsService;
        private const string CredentialType = "trino";

        public DeleteTrinoUserHandler(
            ILogger<DeleteTrinoUserHandler> logger,
            ILdapUserManagementService ldapUserManagementService,
            IVaultCredentialsService vaultCredentialsService,
            IEphemeralCredentialsService ephemeralCredentialsService)
        {
            _logger = logger;
            _ldapUserManagementService = ldapUserManagementService;
            _vaultCredentialsService = vaultCredentialsService;
            _ephemeralCredentialsService = ephemeralCredentialsService;
        }

        public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("DeleteTrinoUserHandler started. processInstance={ProcessInstanceKey}", job.ProcessInstanceKey);

            try
            {
                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);
                var envListJson = variables["envList"]?.ToString();
                var envList = JsonSerializer.Deserialize<List<CredentialsCamundaOutput>>(envListJson);

                if (envList?.FirstOrDefault() == null)
                {
                    var errorMsg = "No credential information found in envList";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                string? username = envList.Where(x => x.env.ToLower().Contains("username")).FirstOrDefault()?.value?.ToString();
                string? project = variables["project"]?.ToString();
                string? user = variables["user"]?.ToString();

                var submissionIdEntry = envList.FirstOrDefault(x => x.env.ToLower().Contains("submissionid"));
                string? submissionId = submissionIdEntry?.value?.ToString();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(project) || string.IsNullOrEmpty(user))
                {
                    var errorMsg = "Missing required credentials; cannot proceed with Trino user deletion.";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation("Attempting to delete LDAP user: {Username}", username);

                var result = await _ldapUserManagementService.DeleteUserAsync(username);

                if (!result.Success)
                {
                    var errorMsg = $"Failed to delete LDAP user {username}: {result.ErrorMessage}";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation("Successfully deleted LDAP user: {Username}", username);

                if (!string.IsNullOrEmpty(submissionId) && int.TryParse(submissionId, out int submissionIdInt))
                {
                    var vaultPath = await _ephemeralCredentialsService.GetVaultPathBySubmissionIdAsync(submissionIdInt, CredentialType, cancellationToken);

                    if (!string.IsNullOrEmpty(vaultPath))
                    {
                        _logger.LogInformation("Removing credentials from Vault at path: {VaultPath}", vaultPath);

                        var vaultDeleteResult = await _vaultCredentialsService.RemoveCredentialAsync(vaultPath);

                        if (vaultDeleteResult)
                        {
                            _logger.LogInformation("Successfully removed credentials from Vault at path: {VaultPath}", vaultPath);

                            await _ephemeralCredentialsService.UpdateCredentialExpirationAsync(vaultPath, cancellationToken);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to remove credentials from Vault at path: {VaultPath}", vaultPath);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No vaultPath found for submissionId: {SubmissionId}", submissionId);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid or missing submissionId: {SubmissionId}", submissionId);
                }

                sw.Stop();
                _logger.LogInformation("DeleteTrinoUserHandler took {Seconds} seconds", sw.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error in DeleteTrinoUserHandler: {ex.Message}";
                _logger.LogError(ex, errorMsg);

                sw.Stop();
                _logger.LogInformation("DeleteTrinoUserHandler took {Seconds} seconds", sw.Elapsed.TotalSeconds);

                throw;
            }
        }
    }
}