using Tre_Camunda.Services;
using BL.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zeebe.Client.Accelerator.Attributes;
using Zeebe.Client.Accelerator.Abstractions;
using System.Diagnostics;
using System.Text.Json;
using Tre_Camunda.Settings;
using Microsoft.Extensions.Options;


namespace Tre_Camunda.ProcessHandlers
{
    [JobType("create-trino-user")]
    public class CreateTrinoUserHandler: IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
    {
        private readonly ILogger<CreatePostgresUserHandler> _logger;
        private readonly ILdapUserManagementService _ldapUserManagementService;
        private readonly CredentialSettings _config;

        public CreateTrinoUserHandler(ILogger<CreatePostgresUserHandler> logger, ILdapUserManagementService ldapUserManagementService, IOptions<CredentialSettings> config)
        {
            _logger = logger;
            _ldapUserManagementService = ldapUserManagementService;
            _config = config.Value;
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

                var credentialInfo = envList?.FirstOrDefault();
                if (credentialInfo == null)
                {
                    var errorMsg = "No credential information found in envList";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                var username = credentialInfo.ContainsKey("value") ? credentialInfo["value"]?.ToString()
                    : credentialInfo.ContainsKey("username")
                    ? credentialInfo["username"]?.ToString() : null;

                var project = variables["project"]?.ToString();
                var user = variables["user"]?.ToString();

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
                    var userExpirationPeriod = _config.UserExpirationPeriod;
                    var outputVariables = new Dictionary<string, object>
                    {
                        
                        ["credentialData"] = new Dictionary<string, object>
                        {
                            ["username"] = username,
                            ["password"] = password,
                            ["credentialType"] = "trino",
                            ["project"] = project,
                            ["userId"] = user,
                            ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            ["expiresAt"] = DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            ["ldapDn"] = $"cn={username},ou=Users,dc=camundaephemeral,dc=local"
                        },

                        ["vaultPath"] = $"trino/{project}/{user}/{username}",
                        ["trinoUsername"] = username,
                        ["userExpirationPeriod"] = userExpirationPeriod
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
    }
}
