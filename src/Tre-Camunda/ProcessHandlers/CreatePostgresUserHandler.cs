
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
                    string? project = variables["project"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "");
;                   string? user = variables["user"]?.ToString().Replace("[", "").Replace("]", "");

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(project))
                    {
                        var errorMsg = "Missing Credentials cannot proceed";
                        _logger.LogError(errorMsg);
                        throw new Exception(errorMsg);

                    }
                    else
                    {

                        var password = GenerateSecurePassword();


                        var schemaPermissions = new List<SchemaPermission>
                        {
                            new SchemaPermission
                            {
                                SchemaName = project,
                                Permissions = DatabasePermissions.Read | DatabasePermissions.Write | DatabasePermissions.CreateTables
                            }

                        };


                        var createUserRequest = new CreateUserRequest
                        {
                            Username = username,
                            Password = password,
                            Server = server,
                            Datasbasename = database,
                            Port = port,
                            SchemaPermissions = schemaPermissions
                        };


                        var result = await _postgreSQLUserManagementService.CreateUserAsync(createUserRequest);

                        if (result.Success)
                        {
                            var credentialData = new Dictionary<string, object>();

                            foreach (var credential in envList)
                            {
                                var CredentialEnv = new CredentialsVault();


                                CredentialEnv.env = credential.env;
                                if (credential.value.Contains(password))
                                {
                                    credential.value = password;
                                }
                                else
                                {
                                    CredentialEnv.value = credential.value;
                                }

                                credentialData.Add(CredentialEnv.env, CredentialEnv.value);

                            }



                            var vaultPath = $"postgres/{project}/{user}/{username}";


                            var success = await _vaultCredentialsService.AddCredentialAsync(vaultPath, credentialData);
                            if (!success)
                            {
                                var errorMsg = $"Failed to store credential in Vault at path: {vaultPath}";
                                _logger.LogError(errorMsg);
                                throw new Exception(errorMsg);
                            }                           

                            _logger.LogInformation($"Successfully created PostgreSQL user: {username} for project: {project}");

                            SW.Stop();
                            _logger.LogInformation($"CreatePostgresUserHandler took {SW.Elapsed.TotalSeconds} seconds");

                            var outputVariables = new Dictionary<string, object>
                            {
                                ["credentialData"] = new Dictionary<string, object>
                                {
                                    ["username"] = username,
                                    ["credentialType"] = "postgres",
                                    ["project"] = project                                   
                                }                           
                            };


                            return outputVariables;
                        }
                        else
                        {

                            var errorMsg = $"Failed to create PostgreSQL user: {result.ErrorMessage}";
                            _logger.LogError(errorMsg);
                            throw new Exception(errorMsg);
                        }
                    }
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

