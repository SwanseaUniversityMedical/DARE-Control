using Microsoft.EntityFrameworkCore;
using Tre_Credentials.DbContexts;

namespace Tre_Camunda.Services
{
    public class EphemeralCredentialsService : IEphemeralCredentialsService
    {
        private readonly CredentialsDbContext _credentialsDbContext;
        private readonly ILogger<EphemeralCredentialsService> _logger;

        public EphemeralCredentialsService(
            CredentialsDbContext credentialsDbContext,
            ILogger<EphemeralCredentialsService> logger)
        {
            _credentialsDbContext = credentialsDbContext;
            _logger = logger;
        }

        public async Task<bool> UpdateCredentialExpirationAsync(string vaultPath, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Updating audit trail for vaultPath: {vaultPath}");

                var credential = await _credentialsDbContext.EphemeralCredentials
                    .FirstOrDefaultAsync(c => c.VaultPath == vaultPath && !c.ExpiredAt.HasValue, cancellationToken);

                if (credential != null)
                {
                    credential.ExpiredAt = DateTime.UtcNow;
                    await _credentialsDbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation($"Successfully updated audit trail for vaultPath: {vaultPath}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"No credential found in database for vaultPath: {vaultPath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating audit trail for vaultPath: {vaultPath}");
                return false;
            }
        }

        public async Task<string?> GetVaultPathBySubmissionIdAsync(int submissionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var credential = await _credentialsDbContext.EphemeralCredentials
                    .Where(c => c.SubmissionId == submissionId && !c.ExpiredAt.HasValue)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                return credential?.VaultPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving vaultPath for submissionId: {submissionId}");
                return null;
            }
        }

    }

}
