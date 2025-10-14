using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Tre_Camunda.Models;
using Tre_Camunda.Services;
using Tre_Camunda.Settings;
using Tre_Credentials.DbContexts;
using Tre_Credentials.Migrations;
using Tre_Credentials.Models;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;



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




//namespace Tre_Camunda.ProcessHandlers
//{
//    [JobType("create-trino-user")]
//    public class CreateTrinoUserHandler: IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
//    {
//        private readonly ILogger<CreatePostgresUserHandler> _logger;
//        private readonly ILdapUserManagementService _ldapUserManagementService;
//        private readonly CredentialsDbContext _credsDbContext;


//        public CreateTrinoUserHandler(ILogger<CreatePostgresUserHandler> logger, ILdapUserManagementService ldapUserManagementService, CredentialsDbContext credsDbContext)
//        {
//            _logger = logger;
//            _ldapUserManagementService = ldapUserManagementService;
//            _credsDbContext = credsDbContext;

//        }   

//        public async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
//        {
//            var SW = new Stopwatch();
//            SW.Start();

//            _logger.LogDebug($"CreateTrinoUserHandler started for process instance {job.ProcessInstanceKey}");

//            try
//            {
//                var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);

//                var envListJson = variables["envList"]?.ToString();
//                var envList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(envListJson);

//                var usernameInfo = envList?.FirstOrDefault();
//                var submissionInfo = envList?.LastOrDefault();

//                if (usernameInfo == null || submissionInfo == null)
//                {
//                    var errorMsg = "No credential information found in envList";
//                    _logger.LogError(errorMsg);
//                    throw new Exception(errorMsg);
//                }

//                var username = usernameInfo.ContainsKey("value") ? usernameInfo["value"]?.ToString()
//                    : usernameInfo.ContainsKey("username")
//                    ? usernameInfo["username"]?.ToString() : null;

//                var submissionId = submissionInfo.ContainsKey("value") ? submissionInfo["value"]?.ToString()
//                  : submissionInfo.ContainsKey("submissionId")
//                  ? submissionInfo["submissionId"]?.ToString() : null;

//                var submissionGuid = int.Parse(submissionId);

//                string credentialType = "trino";

//                var project = variables["project"]?.ToString();
//                var user = variables["user"]?.ToString();

//                var processInstanceKey = job.ProcessInstanceKey;

//                _logger.LogInformation($"Creating Trino user for Submission: {submissionId}, Process: {processInstanceKey}");

//                if (string.IsNullOrEmpty(username))
//                {
//                    var errorMsg = "Username not found in DMN result";
//                    _logger.LogError(errorMsg);
//                    throw new Exception(errorMsg);
//                }

//                var credRow = await _credsDbContext.EphemeralCredentials.FirstOrDefaultAsync(e => e.SubmissionId == submissionGuid && e.ProcessInstanceKey == processInstanceKey && e.CredentialType == credentialType); 
//                if(credRow == null)
//                {
//                    credRow = new EphemeralCredential
//                    {
//                        SubmissionId = submissionGuid,
//                        ParentProcessInstanceKey = null,
//                        ProcessInstanceKey = processInstanceKey,
//                        CredentialType = credentialType,
//                        CreatedAt = null,
//                        IsProcessed = false,
//                        SuccessStatus = null
//                    };
//                    _credsDbContext.EphemeralCredentials.Add(credRow);
//                    await _credsDbContext.SaveChangesAsync(cancellationToken);
//                }

//                var password = GenerateSecurePassword();

//                var createUserRequest = new CreateUserRequest
//                {
//                    Username = username,
//                    Password = password,
//                    CanLogin = true,
//                    CanCreateDb = false,
//                    CanCreateRole = false                    
//                };

//                var result = await _ldapUserManagementService.CreateUserAsync(createUserRequest);

//                if (result.Success)

//                {
//                    credRow.SuccessStatus = SuccessStatus.Success;
//                    credRow.CreatedAt = DateTime.UtcNow;
//                    credRow.ErrorMessage = null;
//                    await _credsDbContext.SaveChangesAsync(cancellationToken);


//                    var userId = CleanDnValue(user);
//                    var jobId = submissionId;

//                    var outputVariables = new Dictionary<string, object>
//                    {                      
//                        ["credentialData"] = new Dictionary<string, object>
//                        {
//                            ["username"] = username,
//                            ["password"] = password,
//                            ["credentialType"] = credentialType,
//                            ["project"] = project,
//                            ["user_id"] = userId,
//                            ["job_id"] = jobId,
//                            ["createdAt"] = credRow.CreatedAt,
//                            ["expiresAt"] = DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ"),
//                            ["ldapDn"] = $"cn={username},ou=Users,dc=camundaephemeral,dc=local"                            
//                        },
//                        ["submissionId"] = submissionId,
//                        ["processInstanceKey"] = processInstanceKey,
//                        ["vaultPath"] = $"trino/{userId}/{jobId}/{project}", 
//                        ["trinoUsername"] = username 
//                    };

//                    _logger.LogInformation($"Successfully created Trino user: {username} for project: {project}");

//                    SW.Stop();
//                    _logger.LogInformation($"CreateTrinoUserHandler took {SW.Elapsed.TotalSeconds} seconds");

//                    return outputVariables;
//                }
//                else
//                {
//                    var errorMsg = $"Failed to create Trino user: {result.ErrorMessage}";
//                    credRow.SuccessStatus = SuccessStatus.Error;
//                    credRow.ErrorMessage = errorMsg;
//                    await _credsDbContext.SaveChangesAsync(cancellationToken);                  
//                    throw new Exception(errorMsg);
//                }
//            }
//            catch (Exception ex)
//            {
//                var errorMsg = $"Unexpected error in CreateTrinoUserHandler: {ex.Message}";
//                _logger.LogError(ex, errorMsg);

//                    var processInstanceKey = job.ProcessInstanceKey;                   
//                    var credRow = await _credsDbContext.EphemeralCredentials.FirstOrDefaultAsync(e => e.ProcessInstanceKey == processInstanceKey && e.CredentialType == "trino", cancellationToken);
//                    credRow.SuccessStatus = SuccessStatus.Error;
//                    credRow.ErrorMessage = ex.Message;
//                    await _credsDbContext.SaveChangesAsync(cancellationToken);
//                SW.Stop();
//                _logger.LogInformation($"CreateTrinoUserHandler took {SW.Elapsed.TotalSeconds} seconds");

//                throw;

//            }

//        }

//        private string GenerateSecurePassword()
//        {
//            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
//            var random = new Random();
//            return new string(Enumerable.Repeat(chars, 16)
//                .Select(s => s[random.Next(s.Length)]).ToArray());
//        }

//        private static string CleanDnValue(string value)
//        {
//            if (string.IsNullOrEmpty(value)) return value;

//            var charsToRemove = new[] { '[', ']', '\\', '"' };
//            var sb = new StringBuilder();
//            foreach (var c in value)
//            {
//                if (!charsToRemove.Contains(c))
//                {
//                    sb.Append(c);
//                }
//            }
//            return sb.ToString();
//        }
//    }
//}
