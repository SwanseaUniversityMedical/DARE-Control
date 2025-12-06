using Microsoft.Extensions.Logging;
using Tre_Camunda.Services;
using Zeebe.Client.Accelerator.Attributes;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("delete-minio-user")]
    public class DeleteMinioSecretHandler : DeleteCredentialHandlerBase
    {
        private readonly IMinioManagementService _minioManagementService;
        protected override string CredentialType => "minio";

        public DeleteMinioSecretHandler(
            ILogger<DeleteMinioSecretHandler> logger,
            IMinioManagementService minioManagementService,
            IVaultCredentialsService vaultCredentialsService,
            IEphemeralCredentialsService ephemeralCredentialsService)
            : base(logger, vaultCredentialsService, ephemeralCredentialsService)
        {
            _minioManagementService = minioManagementService;
        }

        /// <summary>
        /// Delete MinIO user using the MinIO management service
        /// </summary>
        protected override async Task<bool> DeleteUserAsync(string? accessKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(accessKey))
                return false;

            var result = await _minioManagementService.DeleteUserAsync(accessKey, cancellationToken);
            return result.Success;
        }

        /// <summary>
        /// Check if MinIO user exists
        /// </summary>
        protected override async Task<bool> CheckUserExistAsync(string accessKey)
        {
            var result = await _minioManagementService.UserExistsAsync(accessKey);
            return result;
        }
    }
}
