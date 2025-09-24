using Tre_Camunda.Services;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;
using BL.Services;
using BL.Services.Contract;
using System.Diagnostics;
using System.Text.Json;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("delete-postgres-user")]
    public class DeletePostgresUserHandler : IAsyncZeebeWorker
    {
        private readonly ILogger<DeletePostgresUserHandler> _logger;
        private readonly IPostgreSQLUserManagementService _postgresUserManagementService;

        public DeletePostgresUserHandler(ILogger<DeletePostgresUserHandler> logger, IPostgreSQLUserManagementService postgresUserManagementService)
        {
            _logger = logger;
            _postgresUserManagementService = postgresUserManagementService;
        }

        public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var SW = new Stopwatch();
            SW.Start();

            _logger.LogDebug($"DeletePostgresUserHandler started for process instance {job.ProcessInstanceKey}");

            try
            {
                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);
                var username = variables != null && variables.TryGetValue("trinoUsername", out var u)  //Change it to use postgres username
                    ? u?.ToString()
                    : null;

                if (string.IsNullOrWhiteSpace(username))
                {
                    var errorMsg = "postgresUsername not found in process variables";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation($"Attempting to delete postgres user: {username}");

                var result = await _postgresUserManagementService.DropUserAsync(username);

                if (!result)
                {
                    var errorMsg = $"Failed to delete postgres user {username}";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation($"Successfully deleted postgres user: {username}");
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
