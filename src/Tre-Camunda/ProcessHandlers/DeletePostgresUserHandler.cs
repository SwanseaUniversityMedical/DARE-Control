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

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(project))
                    {
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