using BL.Services;
using BL.Services.Contract;


namespace TRE_API.Services
{
    public class CredentialProcessingService : ICredentialProcessingService
    {

        private readonly IVaultCredentialsService _vaultCredentialService; //Shift VaultService to BL to access here
        private readonly ILogger<CredentialProcessingService> _logger;

        public CredentialProcessingService(IVaultCredentialsService vaultService, ILogger<CredentialProcessingService> logger)
        {
            _vaultCredentialService = vaultService;
            _logger = logger;
        }

        public async Task ProcessCredentialsAsync(string submissionId, string vaultPath)
        {
            try
            {
                _logger.LogInformation($"Processing credentials for submission {submissionId} from vault {vaultPath}");
               
                var credentials = await _vaultCredentialService.GetCredentialAsync(vaultPath);

                if (credentials == null || !credentials.Any())
                {
                    _logger.LogWarning($"No credentials found for submission {submissionId} at {vaultPath}");
                    return;
                }

                _logger.LogInformation($"Successfully retrieved credentials for submission {submissionId}");

                
                //Call DoAgentWork method that we define here
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error in Credential Processing: {ex.Message}";
                _logger.LogError(ex, errorMsg);

                throw;
            }
        }
    }
}
