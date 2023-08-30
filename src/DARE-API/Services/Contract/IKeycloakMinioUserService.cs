namespace DARE_API.Services.Contract
{
    public interface IKeycloakMinioUserService
    {
        Task<bool> SetMinioUserAttribute(string accessToken, string userName, string attributeName, string NewAttribute);
        Task<bool> RemoveMinioUserAttribute(string accessToken, string userName, string attributeName, string NewAttribute);
    }
}

