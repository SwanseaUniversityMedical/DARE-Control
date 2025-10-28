using Tre_Camunda.Services;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;
using System.Diagnostics;
using System.Text.Json;
using Tre_Camunda.Models;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("delete-postgres-user")]
    public class DeletePostgresUserHandler : IAsyncZeebeWorker
    {
        private readonly ILogger<DeletePostgresUserHandler> _logger;
        private readonly IPostgreSQLUserManagementService _postgresUserManagementService;
        private readonly IVaultCredentialsService _vaultCredentialsService;
        private readonly IEphemeralCredentialsService _ephemeralCredentialsService;
        private const string CredentialType = "postgres";

        public DeletePostgresUserHandler(ILogger<DeletePostgresUserHandler> logger,
            IPostgreSQLUserManagementService postgresUserManagementService,
            IVaultCredentialsService vaultCredentialsService,
            IEphemeralCredentialsService ephemeralCredentialsService)
        {
            _logger = logger;
            _postgresUserManagementService = postgresUserManagementService;
            _vaultCredentialsService = vaultCredentialsService;
            _ephemeralCredentialsService = ephemeralCredentialsService;
        }

        public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var SW = new Stopwatch();
            SW.Start();

            _logger.LogDebug($"DeletePostgresUserHandler started for process instance {job.ProcessInstanceKey}");

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
                else
                {

                    string? username = envList.Where(x => x.env.ToLower().Contains("username")).FirstOrDefault().value.ToString();
                    string? database = envList.Where(x => x.env.ToLower().Contains("database")).FirstOrDefault().value.ToString();
                    string? server = envList.Where(x => x.env.ToLower().Contains("server")).FirstOrDefault().value.ToString();
                    string? port = envList.Where(x => x.env.ToLower().Contains("port")).FirstOrDefault().value.ToString();
                    string? project = variables["project"]?.ToString();
                    string? user = variables["user"]?.ToString();

                    var submissionIdEntry = envList.FirstOrDefault(x => x.env.ToLower().Contains("submissionid"));
                    string? submissionId = submissionIdEntry?.value?.ToString();
                    //string? submissionId = variables["submissionId"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "");
                    //var submissionId = variables["submissionId"]?.ToString();

                    if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(database) || !string.IsNullOrEmpty(server) || !string.IsNullOrEmpty(port) || !string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(project))
                    {
                        // Check if user exists before attempting deletion
                        var userExists = await _postgresUserManagementService.UserExistsAsync(username);
                        if (!userExists)
                        {
                            _logger.LogInformation("Postgres user {Username} does not exist, skipping deletion", username);
                            SW.Stop();
                            return;
                        }
                        
                        _logger.LogInformation($"Attempting to delete postgres user: {username}");
                        
                        var deleteUserResult = await _postgresUserManagementService.DropUserAsync(username);

                        if (!deleteUserResult)
                        {
                            var errorMsg = $"Failed to delete postgres user {username}";
                            _logger.LogError(errorMsg);
                            throw new Exception(errorMsg);
                        }

                        _logger.LogInformation($"Successfully deleted postgres user: {username}");

                        if (!string.IsNullOrEmpty(submissionId) && int.TryParse(submissionId, out int submissionIdInt))
                        {
                            var vaultPath = await _ephemeralCredentialsService.GetVaultPathBySubmissionIdAsync(submissionIdInt, CredentialType, cancellationToken);

                            if (!string.IsNullOrEmpty(vaultPath))
                            {
                                _logger.LogInformation($"Removing credentials from Vault at path: {vaultPath}");

                                var vaultDeleteResult = await _vaultCredentialsService.RemoveCredentialAsync(vaultPath);

                                if (vaultDeleteResult)
                                {
                                    _logger.LogInformation($"Successfully removed credentials from Vault at path: {vaultPath}");

                                    await _ephemeralCredentialsService.UpdateCredentialExpirationAsync(vaultPath, cancellationToken);
                                }
                                else
                                {
                                    _logger.LogWarning($"Failed to remove credentials from Vault at path: {vaultPath}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"No vaultPath found for submissionId: {submissionId}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Invalid or missing submissionId: {submissionId}");
                        }

                        SW.Stop();
                        _logger.LogInformation($"DeletePostgresUserHandler took {SW.Elapsed.TotalSeconds} seconds");
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error in DeletePostgresUserHandler: {ex.Message}";
                _logger.LogError(ex, errorMsg);

                SW.Stop();
                _logger.LogInformation($"DeletePostgresUserHandler took {SW.Elapsed.TotalSeconds} seconds");

                throw;
            }

        }
    }
}