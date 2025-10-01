using BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tre_Camunda.Services
{
    public interface IPostgreSQLUserManagementService
    {
        Task<UserCreationResult> CreateUserAsync(CreateUserRequest request);
        Task<bool> UserExistsAsync(string username);
        Task<bool> DropUserAsync(string username);
        Task<bool> GrantSchemaPermissionsAsync(string username, string schemaName, DatabasePermissions permissions);
        Task<bool> RevokeSchemaPermissionsAsync(string username, string schemaName);
        Task<List<string>> GetUserSchemasAsync(string username);
    }
}
