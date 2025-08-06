using Amazon.S3.Model;
using Microsoft.IdentityModel.Tokens;
using BL.Models;
using System.DirectoryServices.Protocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;


namespace Tre_Camunda.Services
{
    public class LdapUserManagementService : ILdapUserManagementService
    {
        private readonly LdapSettings _config;
        private readonly ILogger<LdapUserManagementService> _logger;

        public LdapUserManagementService(IOptions<LdapSettings> config, ILogger<LdapUserManagementService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        private LdapConnection CreateConnection()
        {
            var identifier = new LdapDirectoryIdentifier(_config.Host, _config.Port);

            var connection = new LdapConnection(identifier);

            if (_config.UseSSL)
            {
                connection.SessionOptions.SecureSocketLayer = true;
            }

            connection.AuthType = AuthType.Basic;
            connection.Credential = new NetworkCredential(_config.AdminDn, _config.AdminPassword);

            return connection;
        }

        public async Task<UserCreationResult> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Bind();
                
                //Null check
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    return UserCreationResult.Error("Username and password are required");

                //Check if user already exists
                if (await UserExistsAsync(request.Username))
                    return UserCreationResult.Error($"User {request.Username} already exists");
                var userDn = $"cn={request.Username},{_config.UserOu},{_config.BaseDn}";

                var schemaPermissions = request.SchemaPermissions.Select(p => $"{p.SchemaName}:{(int)p.Permissions}").ToArray();

                var addRequest = new AddRequest(userDn, new DirectoryAttribute[]
                {
                    new("objectClass", "inetOrgPerson"), //Internet Organisational Person
                    new("cn", request.Username), //Common name
                    new("sn", request.Username), //Surname
                    new("userPassword", request.Password),
                    new("uid", request.Username), //User ID

                   new("businessCategory", schemaPermissions)
                });

                var response = (AddResponse)connection.SendRequest(addRequest);

                if (response.ResultCode == ResultCode.Success)
                {
                    _logger.LogInformation($"user {request.Username} created successfully");
                    return UserCreationResult.Ok();
                }
                else
                {
                    _logger.LogError($"Failed to create User {request.Username}:{response.ErrorMessage}");
                    return UserCreationResult.Error(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Exception while creating user {request.Username}");
                return UserCreationResult.Error(ex.Message);
            }
        }

        public async Task<UserCreationResult> DeleteUserAsync(string username)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Bind();

                //Check if user exists

                var userDn = $"cn={username}, {_config.UserOu}, {_config.BaseDn}";
                var deleteRequest = new DeleteRequest(userDn);

                var response = (DeleteResponse)connection.SendRequest(deleteRequest);

                if(response.ResultCode == ResultCode.Success)
                {
                    _logger.LogInformation($"User {username} deleted successfully");
                    return UserCreationResult.Ok();
                }
                else
                {
                    _logger.LogError($"Failed to delete user {username}: {response.ErrorMessage}");
                    return UserCreationResult.Error(response.ErrorMessage);
                }
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, $"Exception while deleting user {username}" );
                return UserCreationResult.Error(ex.Message );
            }
        }

        private async Task<bool> UserExistsAsync(string username)
        {
            using var connection = CreateConnection();
            connection.Bind();

            var searchRequest = new SearchRequest(
                $"{_config.UserOu},{_config.BaseDn}",
                $"(cn={username})",
                SearchScope.OneLevel,
                "cn");

            var response = (SearchResponse)connection.SendRequest(searchRequest);
            return response.Entries.Count > 0;
        }
    }
}
