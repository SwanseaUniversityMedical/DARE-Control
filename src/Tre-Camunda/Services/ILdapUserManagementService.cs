using Tre_Camunda.Models;

namespace Tre_Camunda.Services
{
    public interface ILdapUserManagementService
    {
        Task<UserCreationResult> CreateUserAsync(CreateUserRequest request);

        Task<UserCreationResult> DeleteUserAsync(string username);        
    }
}
