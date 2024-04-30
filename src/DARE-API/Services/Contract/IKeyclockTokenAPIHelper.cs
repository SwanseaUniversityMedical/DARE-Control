using Microsoft.AspNetCore.Authentication;

namespace DARE_API.Services.Contract
{
    public interface IKeyclockTokenAPIHelper
    {
        Task<string> GetTokenForUser(string username, string password, string requiredRole);


    }
}
