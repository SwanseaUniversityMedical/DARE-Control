using System.Diagnostics;
using Tre_Credentials.Models.Zeebe;
using Tre_Camunda.Models;
using Tre_Camunda.Services;
using Tre_Credentials.DbContexts;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Tre_Camunda.ProcessHandlers
{
    [JobType("create-minio-secret")]
    public class CreateMinioSecretHandler : CreateCredentialHandlerBase
    {
        private readonly IMinioManagementService _minioManagementService;

        public CreateMinioSecretHandler(
            IMinioManagementService minioManagementService,
            ILogger<CreateMinioSecretHandler> logger,
            IVaultCredentialsService vaultCredentialsService,
            CredentialsDbContext credentialsDbContext)
            : base(vaultCredentialsService, credentialsDbContext, logger)
        {
            _minioManagementService = minioManagementService;
        }

        public override async Task<Dictionary<string, object>> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("CreateMinioSecretHandler started. processInstance={ProcessInstanceKey}", job.ProcessInstanceKey);

            string? submissionId = null;
            long? parentProcessKey = null;
            long processInstanceKey = job.ProcessInstanceKey;

            try
            {
                var extraction = ExtractCredentials(job);
                submissionId = extraction.SubmissionId;
                parentProcessKey = extraction.ParentProcessKey;

                if (extraction.EnvList?.FirstOrDefault() == null)
                {
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "minio",
                        "No credential information found in envList");
                    return CreateStatusResponse("ERROR: Missing credentials, cannot proceed.");
                }

                string? accessKey = extraction.EnvList
                    .Where(x => x.env.Equals("accessKey", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault()?.value?.ToString();

                string? endPoint = extraction.EnvList
                    .FirstOrDefault(x => x.env.Equals("endPoint", StringComparison.OrdinalIgnoreCase))
                    ?.value?.ToString();

                string? bucketName = extraction.Project;

                if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(bucketName) ||
                    string.IsNullOrEmpty(extraction.User) || string.IsNullOrEmpty(extraction.Project))
                {
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "minio",
                        "Missing credentials; cannot proceed with MinIO secret creation.");
                    return CreateStatusResponse("ERROR: Missing credentials, cannot proceed.");
                }

                var secretKey = GenerateSecurePassword();

                var result = await _minioManagementService.CreateUserAsync(accessKey, secretKey, bucketName, cancellationToken);
                if (!result.Success)
                {
                    await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "minio",
                        $"Failed to create MinIO secret: {result.Error}");
                    return CreateStatusResponse("ERROR: Failed credential creation");
                }

                var credentialData = BuildCredentialData(extraction.EnvList, secretKey);

                if (credentialData.ContainsKey("endPoint") && string.IsNullOrEmpty(credentialData["endPoint"]?.ToString()))
                {
                    credentialData["endPoint"] = endPoint ?? string.Empty;
                }

                string vaultPath = $"minio/{extraction.User}/{submissionId}/{extraction.Project}";
                if (!await StoreInVaultAsync(submissionId, parentProcessKey, processInstanceKey, vaultPath, credentialData, "minio"))
                    return CreateStatusResponse("ERROR: Credential store in vault failed");

                await CreateCredentialsReadyMessageAsync(submissionId, parentProcessKey, processInstanceKey, vaultPath, "minio");

                _logger.LogInformation("Successfully created MinIO secret: {AccessKey} for project: {Project}",
                    accessKey, extraction.Project);
                return CreateStatusResponse($"OK: MinIO secret '{accessKey}' created for project '{extraction.Project}'.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateMinioSecretHandler. processInstance={ProcessInstanceKey}",
                    processInstanceKey);
                await RecordErrorAsync(submissionId, parentProcessKey, processInstanceKey, "minio",
                    $"Unexpected error: {ex.Message}");
                return CreateStatusResponse("Unexpected Error in MinIO handler");
            }
            finally
            {
                if (sw.IsRunning) sw.Stop();
                _logger.LogInformation("CreateMinioSecretHandler took {Seconds} seconds", sw.Elapsed.TotalSeconds);
            }
        }
    }
}
