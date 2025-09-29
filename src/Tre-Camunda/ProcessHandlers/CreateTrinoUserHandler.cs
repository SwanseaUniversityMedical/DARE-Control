using Tre_Camunda.Services;
using BL.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zeebe.Client.Accelerator.Attributes;
using Zeebe.Client.Accelerator.Abstractions;
using System.Diagnostics;
using System.Text.Json;
using Amazon.Runtime.Internal;
using System.Text;
using Tre_Camunda.Settings;
using Microsoft.Extensions.Options;



namespace Tre_Camunda.ProcessHandlers
{
    [JobType("create-trino-user")]
    public class CreateTrinoUserHandler: IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
    {
        private readonly ILogger<CreatePostgresUserHandler> _logger;
        private readonly ILdapUserManagementService _ldapUserManagementService;
        

        public CreateTrinoUserHandler(ILogger<CreatePostgresUserHandler> logger, ILdapUserManagementService ldapUserManagementService)
        {
            _logger = logger;
            _ldapUserManagementService = ldapUserManagementService;
           
        }   

        public async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var SW = new Stopwatch();
            SW.Start();

            _logger.LogDebug($"CreateTrinoUserHandler started for process instance {job.ProcessInstanceKey}");

            try
            {
                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);

                var envListJson = variables["envList"]?.ToString();
                var envList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(envListJson);

                var usernameInfo = envList?.FirstOrDefault();
                var submissionInfo = envList?.LastOrDefault();

                if (usernameInfo == null || submissionInfo == null)
                {
                    var errorMsg = "No credential information found in envList";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                var username = usernameInfo.ContainsKey("value") ? usernameInfo["value"]?.ToString()
                    : usernameInfo.ContainsKey("username")
                    ? usernameInfo["username"]?.ToString() : null;

                var submissionId = submissionInfo.ContainsKey("value") ? submissionInfo["value"]?.ToString()
                  : submissionInfo.ContainsKey("submissionId")
                  ? submissionInfo["submissionId"]?.ToString() : null;

                var project = variables["project"]?.ToString();
                var user = variables["user"]?.ToString();
     
                var processInstanceKey = job.ProcessInstanceKey;

                _logger.LogInformation($"Creating Trino user for Submission: {submissionId}, Process: {processInstanceKey}");

                if (string.IsNullOrEmpty(username))
                {
                    var errorMsg = "Username not found in DMN result";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                var password = GenerateSecurePassword();

                var createUserRequest = new CreateUserRequest
                {
                    Username = username,
                    Password = password,
                    CanLogin = true,
                    CanCreateDb = false,
                    CanCreateRole = false,
                    //ExpiryDate = DateTime.UtcNow.AddHours(24) 
                };

                var result = await _ldapUserManagementService.CreateUserAsync(createUserRequest);

                if (result.Success)

                {
                    var userId = CleanDnValue(user);
                    var jobId = submissionId;
                 
                    var outputVariables = new Dictionary<string, object>
                    {                      
                        ["credentialData"] = new Dictionary<string, object>
                        {
                            ["username"] = username,
                            ["password"] = password,
                            ["credentialType"] = "trino",
                            ["project"] = project,
                            ["user_id"] = userId,
                            ["job_id"] = jobId,
                            ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            ["expiresAt"] = DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            ["ldapDn"] = $"cn={username},ou=Users,dc=camundaephemeral,dc=local"                            
                        },
                        ["submissionId"] = submissionId,
                        ["processInstanceKey"] = processInstanceKey,
                        ["vaultPath"] = $"trino/{userId}/{jobId}/{project}", 
                        ["trinoUsername"] = username 
                    };

                    _logger.LogInformation($"Successfully created Trino user: {username} for project: {project}");

                    SW.Stop();
                    _logger.LogInformation($"CreateTrinoUserHandler took {SW.Elapsed.TotalSeconds} seconds");

                    return outputVariables;
                }
                else
                {
                    var errorMsg = $"Failed to create Trino user: {result.ErrorMessage}";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error in CreateTrinoUserHandler: {ex.Message}";
                _logger.LogError(ex, errorMsg);

                SW.Stop();
                _logger.LogInformation($"CreateTrinoUserHandler took {SW.Elapsed.TotalSeconds} seconds");

                throw;
            }
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
