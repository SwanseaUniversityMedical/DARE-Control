using Tre_Credentials.DbContexts;
using Tre_Credentials.Models;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Storage;
using BL.Services;

namespace TRE_API.Services
{


    public interface IEphemeralCredMonitorService
    {
        Task ProcessAllPendingCredentials();        
    }

    public class EphemeralCredMonitorService : IEphemeralCredMonitorService
    {
        private readonly CredentialsDbContext _credentialsDb;
        private readonly IVaultCredentialsService _vaultService; //Bring VaultService to BL to access here
        private readonly ILogger<EphemeralCredMonitorService> _logger;

        public EphemeralCredMonitorService(CredentialsDbContext credentialsDb, IVaultCredentialsService vaultService, ILogger<EphemeralCredMonitorService> logger)
        {
            _credentialsDb = credentialsDb;
            _vaultService = vaultService;
            _logger = logger;
        }

        public async Task ProcessAllPendingCredentials()
        {
            try
            {
                _logger.LogInformation("Starting check for all pending credentials..."); 
                
                //This should fetch the latest credentials that are ready from the DB
                var pendingMessages = await _credentialsDb.EphemeralCredentials.Where(m => !m.IsProcessed)
                    .OrderBy(m => m.CreatedAt).ToListAsync();

                if (!pendingMessages.Any())
                {
                    _logger.LogInformation("No pending credentials found.");
                    return;
                }

                _logger.LogInformation($"Found pending credential messages");                                
                foreach (var message in pendingMessages)
                {
                    try
                    {
                        await FetchEphemeralCredential(message);                       
                    }
                    catch (Exception ex)
                    {                        
                        _logger.LogError(ex, $"Failed to process credential message for submission {message.SubmissionId} and process Instance {message.ProcessInstanceKey}");                       
                        message.ErrorMessage = ex.Message;
                    }
                }                
                await _credentialsDb.SaveChangesAsync();

                _logger.LogInformation($"Credential check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during credential check");
                throw;
            }
        }

        private async Task FetchEphemeralCredential(EphemeralCredential message)
        {
            _logger.LogInformation($"Processing credentials for Submission: {message.SubmissionId}, ProcessKey: {message.ProcessInstanceKey}, VaultPath: {message.VaultPath}");   
            var credentials = await _vaultService.GetCredentialAsync(message.VaultPath);

            if (credentials == null)
            {
                var errorMsg = $"Credentials not found in vault at path: {message.VaultPath}";                          
                throw new Exception(errorMsg);
            }            

            // Mark as processed
            message.IsProcessed = true;                      

            _logger.LogInformation($"Successfully processed credentials for submission: {message.SubmissionId} and processInstance: {message.ProcessInstanceKey}");

            //Maybe after this, do whatever needs to be done in DoAgentWork
        }

    }
}

