using Tre_Camunda.Services;
using BL.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public DeleteTrinoUserHandler(ILogger<DeleteTrinoUserHandler> logger, ILdapUserManagementService ldapUserManagementService)
        {
            _logger = logger;
            _ldapUserManagementService = ldapUserManagementService;
        }

        public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var SW = new Stopwatch();
            SW.Start();

            _logger.LogDebug($"DeleteTrinoUserHandler started for process instance {job.ProcessInstanceKey}");

            try
            {
                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);                
                var username = variables != null && variables.TryGetValue("trinoUsername", out var u)
                    ? u?.ToString()
                    : null;

                if (string.IsNullOrWhiteSpace(username))
                {
                    var errorMsg = "trinoUsername not found in process variables";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation($"Attempting to delete LDAP user: {username}");

                var result = await _ldapUserManagementService.DeleteUserAsync(username);

                if (!result.Success)
                {
                    var errorMsg = $"Failed to delete LDAP user {username}: {result.ErrorMessage}";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation($"Successfully deleted LDAP user: {username}");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error in DeleteTrinoUserHandler: {ex.Message}";
                _logger.LogError(ex, errorMsg);

                SW.Stop();
                _logger.LogInformation($"DeleteTrinoUserHandler took {SW.Elapsed.TotalSeconds} seconds");

                throw;
            }
        }
    }
}

