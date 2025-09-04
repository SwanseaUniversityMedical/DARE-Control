namespace TRE_API.Services
{
    public interface ICredentialProcessingService
    {
        Task ProcessCredentialsAsync(string submissionId, string vaultPath);
    }
}
