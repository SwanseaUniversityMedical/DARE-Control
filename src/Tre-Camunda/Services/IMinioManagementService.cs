using Tre_Camunda.Models;

namespace Tre_Camunda.Services
{
    public interface IMinioManagementService
    {
        /// <summary>
        /// Create a MinIO user (access key/secret key pair)
        /// </summary>
        /// <param name="accessKey">The access key name</param>
        /// <param name="secretKey">The secret key value (will be generated if not provided)</param>
        /// <param name="bucketName">The bucket name to set policy for</param>
        /// <param name="cancellationToken"></param>
        /// <returns>MinioCommandResult with the created secret information</returns>
        Task<MinioCommandResult> CreateUserAsync(string accessKey, string secretKey, string bucketName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a MinIO user (access key)
        /// </summary>
        /// <param name="accessKey">The access key to delete</param>
        /// <param name="cancellationToken"></param>
        /// <returns>MinioCommandResult indicating success or failure</returns>
        Task<MinioCommandResult> DeleteUserAsync(string accessKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a MinIO user exists
        /// </summary>
        /// <param name="accessKey">The access key to check</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if user exists, false otherwise</returns>
        Task<bool> UserExistsAsync(string accessKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set a policy for a MinIO user on a specific bucket
        /// </summary>
        /// <param name="accessKey">The access key to set policy for</param>
        /// <param name="bucketName">The bucket name</param>
        /// <param name="cancellationToken"></param>
        /// <returns>MinioCommandResult indicating success or failure</returns>
        Task<MinioCommandResult> SetPolicyAsync(string accessKey, string bucketName, CancellationToken cancellationToken = default);
    }
}
