
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;
using static Google.Apis.Requests.BatchRequest;
using Tre_Camunda.Services;
using System.Diagnostics;
using System.Text.Json;
using Tre_Camunda.Models;
using Zeebe.Client.Impl.Commands;
using System.Xml.Linq;



namespace Tre_Camunda.ProcessHandlers
{
       
        [JobType("create-postgres-user")]
        public class CreatePostgresUserHandler : IAsyncZeebeWorkerWithResult<Dictionary<string, object>>
        {
            private readonly IPostgreSQLUserManagementService _postgreSQLUserManagementService;
            private readonly IVaultCredentialsService _vaultCredentialsService;
            private readonly ILogger<CreatePostgresUserHandler> _logger;

            public CreatePostgresUserHandler(
                IPostgreSQLUserManagementService postgresSQLUserManagementService,
                ILogger<CreatePostgresUserHandler> logger,
                IVaultCredentialsService vaultCredentialsService)
            {
                _postgreSQLUserManagementService = postgresSQLUserManagementService;
                _logger = logger;
                _vaultCredentialsService = vaultCredentialsService;
        }

            public async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
            {
                var SW = new Stopwatch();
                SW.Start();

                _logger.LogDebug($"CreatePostgresUserHandler started for process instance {job.ProcessInstanceKey}");

                try
                {
                    
                    var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);
                    
                    var envListJson = variables["envList"]?.ToString();
                    var envList = JsonSerializer.Deserialize<List<CredentialsCamundaOutput>>(envListJson);


                    var credentialInfo = envList?.FirstOrDefault();
                    if (credentialInfo == null)
                    {
                        var errorMsg = "No credential information found in envList";
                        _logger.LogError(errorMsg);
                        throw new Exception(errorMsg);
                    }


                    //var username = credentialInfo.ContainsKey("value")?credentialInfo["value"]?.ToString()
                    //:credentialInfo.ContainsKey("username")
                    //? credentialInfo["username"]?.ToString(): null;


                    var project = variables["project"]?.ToString();
                    var user = variables["user"]?.ToString();

                    if (string.IsNullOrEmpty(username))
                    {
                        var errorMsg = "Username not found in DMN result";
                        _logger.LogError(errorMsg);
                        throw new Exception(errorMsg);
                    }

                    
                    var password = GenerateSecurePassword();

                    
                    var schemaPermissions = new List<SchemaPermission>
                    {
                        new SchemaPermission
                        {
                            SchemaName = "ephemeral", 
                            Permissions = DatabasePermissions.Read | DatabasePermissions.Write | DatabasePermissions.CreateTables 
                        }
                
                    };

                    
                    var createUserRequest = new CreateUserRequest
                    {
                        Username = username,
                        Password = password,
                        Server = "",
                        Datasbasename = "",
                        Port = "",
                        SchemaPermissions = schemaPermissions
                    };

                   
                    var result = await _postgreSQLUserManagementService.CreateUserAsync(createUserRequest);

                    if (result.Success)
                    {

                    var outputVariables = new List<CredentialsVault();
                    var credentialData = new Dictionary<string, object>();

                    foreach (var output in envList)
                    {
                        var outputVariable = new CredentialsVault();
                       
                        
                        outputVariable.env = output.env;
                        if (output.value.Contains(password))  {
                            output.value = password;
                        }
                        else
                        {
                            outputVariable.value = output.value;
                        }

                        credentialData.Add(outputVariable.env, outputVariable.value);
                        outputVariables.Add(outputVariable);
                    }




                    var vaultPath = $"postgres/{project}/{user}/{username}",
                             

                    var success = await _vaultCredentialsService.AddCredentialAsync(vaultPath, credentialData);

                    var outputVariables = new Dictionary<string, object>
                    {
                        //Credential data to store in Vault

                        ["credentialData"] = new Dictionary<string, object>
                        {
                            ["username"] = username,
                            //["password"] = password,
                            ["credentialType"] = "postgres",
                            ["project"] = project,
                            ["userId"] = user,
                            ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            ["expiresAt"] = DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ"), //Not sure, need to confirm
                            ["schemas"] = schemaPermissions.Select(s => s.SchemaName).ToList()
                        },

                        ["vaultPath"] = $"postgres/{project}/{user}/{username}",
                        ["postgresUsername"] = username

                    };

                    _logger.LogInformation($"Successfully created PostgreSQL user: {username} for project: {project}");

                        SW.Stop();
                        _logger.LogInformation($"CreatePostgresUserHandler took {SW.Elapsed.TotalSeconds} seconds");

                        return outputVariables;
                        
                    }
                    else
                    {
                        
                        var errorMsg = $"Failed to create PostgreSQL user: {result.ErrorMessage}";
                        _logger.LogError(errorMsg);
                        throw new Exception(errorMsg);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Unexpected error in CreatePostgresUserHandler: {ex.Message}";
                    _logger.LogError(ex, errorMsg);

                    SW.Stop();
                    _logger.LogInformation($"CreatePostgresUserHandler took {SW.Elapsed.TotalSeconds} seconds");

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

