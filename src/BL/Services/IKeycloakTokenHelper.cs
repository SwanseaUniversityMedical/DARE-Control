namespace BL.Services
{
    public interface IKeycloakTokenHelper
    {
        Task<string> GetTokenForUser(string username, string password, string requiredRole);
    }
}
