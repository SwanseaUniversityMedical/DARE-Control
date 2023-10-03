namespace Data_Egress_API.Services.Contract
{
    public interface IKeycloakTokenAPIHelper
    {
        Task<string> GetTokenForUser(string username, string password, string requiredRole);
    }
}
