namespace TRE_API.Services
{
    public class VaultCredentialsHandler
    {
        public static async Task ProcessCredentials(string submissionId, string vaultPath)
        {
           
            var serviceProvider = ServiceLocator.Current;
            using var scope = serviceProvider.CreateScope();

            var credentialService = scope.ServiceProvider.GetRequiredService<ICredentialProcessingService>();
            await credentialService.ProcessCredentialsAsync(submissionId, vaultPath);
        }
    }
}
