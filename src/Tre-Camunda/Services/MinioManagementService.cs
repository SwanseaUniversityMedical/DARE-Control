using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Tre_Camunda.Models;
using Tre_Camunda.Settings;

namespace Tre_Camunda.Services
{
    public class MinioManagementService : IMinioManagementService
    {
        private readonly MinioSettings _minioSettings;
        private readonly ILogger<MinioManagementService> _logger;
        private bool _isMinioClientInitialized = false;

        public MinioManagementService(MinioSettings minioSettings, ILogger<MinioManagementService> logger)
        {
            _minioSettings = minioSettings;
            _logger = logger;
        }

        /// <summary>
        /// Create a MinIO user (access key/secret key pair) and set policy for bucket
        /// </summary>
        public async Task<MinioCommandResult> CreateUserAsync(string accessKey, string secretKey, string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                var initialized = await EnsureMinioClientInitializedAsync(cancellationToken);
                if (!initialized)
                {
                    return new MinioCommandResult
                    {
                        Success = false,
                        Error = "Failed to initialize MinIO client. Check alias configuration."
                    };
                }

                string createCommand;
                if (string.IsNullOrEmpty(secretKey))
                {
                    createCommand = $"mc admin user add {_minioSettings.Alias} {accessKey}";
                }
                else
                {
                    createCommand = $"mc admin user add {_minioSettings.Alias} {accessKey} {secretKey}";
                }

                var createResult = await ExecuteMinioCommandAsync(createCommand, cancellationToken);

                if (!createResult.Success)
                {
                    _logger.LogWarning("Failed to create MinIO user for access key: {AccessKey}. Error: {Error}", accessKey, createResult.Error);
                    return createResult;
                }

                _logger.LogInformation("MinIO user created successfully for access key: {AccessKey}", accessKey);

                var policyResult = await SetPolicyAsync(accessKey, bucketName, cancellationToken);

                if (!policyResult.Success)
                {
                    _logger.LogWarning("Failed to set policy for access key: {AccessKey} on bucket: {BucketName}", accessKey, bucketName);
                    return policyResult;
                }

                _logger.LogInformation("Policy set successfully for access key: {AccessKey} on bucket: {BucketName}", accessKey, bucketName);

                return new MinioCommandResult
                {
                    Success = true,
                    Output = $"User created and policy set for {accessKey} on bucket {bucketName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating MinIO user for access key: {AccessKey}", accessKey);
                return new MinioCommandResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Delete a MinIO user (access key)
        /// </summary>
        public async Task<MinioCommandResult> DeleteUserAsync(string accessKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var initialized = await EnsureMinioClientInitializedAsync(cancellationToken);
                if (!initialized)
                {
                    return new MinioCommandResult
                    {
                        Success = false,
                        Error = "Failed to initialize MinIO client. Check alias configuration."
                    };
                }

                var command = $"mc admin user remove {_minioSettings.Alias} {accessKey}";
                var result = await ExecuteMinioCommandAsync(command, cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation("MinIO user deleted successfully for access key: {AccessKey}", accessKey);
                }
                else
                {
                    _logger.LogWarning("Failed to delete MinIO user for access key: {AccessKey}. Error: {Error}", accessKey, result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting MinIO user for access key: {AccessKey}", accessKey);
                return new MinioCommandResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Check if a MinIO user exists
        /// </summary>
        public async Task<bool> UserExistsAsync(string accessKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var initialized = await EnsureMinioClientInitializedAsync(cancellationToken);
                if (!initialized)
                {
                    _logger.LogError("Failed to initialize MinIO client for user existence check");
                    return false;
                }

                var command = $"mc admin user info {_minioSettings.Alias} {accessKey}";
                var result = await ExecuteMinioCommandAsync(command, cancellationToken);

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking if MinIO user exists for access key: {AccessKey}", accessKey);
                return false;
            }
        }

        /// <summary>
        /// Set a policy for a MinIO user on a specific bucket
        /// Uses mc admin policy attach command
        /// </summary>
        public async Task<MinioCommandResult> SetPolicyAsync(string accessKey, string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                var initialized = await EnsureMinioClientInitializedAsync(cancellationToken);
                if (!initialized)
                {
                    return new MinioCommandResult
                    {
                        Success = false,
                        Error = "Failed to initialize MinIO client. Check alias configuration."
                    };
                }

                var policyName = $"{bucketName}-policy";
                var policyJson = GenerateBucketPolicy(bucketName);

                var tempPolicyFile = Path.Combine(Path.GetTempPath(), $"{policyName}-{Guid.NewGuid()}.json");
                await File.WriteAllTextAsync(tempPolicyFile, policyJson, cancellationToken);

                try
                {
                    var addPolicyCommand = $"mc admin policy create {_minioSettings.Alias} {policyName} {tempPolicyFile}";
                    var addPolicyResult = await ExecuteMinioCommandAsync(addPolicyCommand, cancellationToken);

                    if (!addPolicyResult.Success && !addPolicyResult.Error.Contains("already exists"))
                    {
                        _logger.LogWarning("Failed to create policy {PolicyName}: {Error}", policyName, addPolicyResult.Error);
                        return addPolicyResult;
                    }

                    var attachCommand = $"mc admin policy attach {_minioSettings.Alias} {policyName} --user {accessKey}";
                    var attachResult = await ExecuteMinioCommandAsync(attachCommand, cancellationToken);

                    if (attachResult.Success)
                    {
                        _logger.LogInformation("Policy {PolicyName} attached successfully to user {AccessKey}", policyName, accessKey);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to attach policy {PolicyName} to user {AccessKey}: {Error}",
                            policyName, accessKey, attachResult.Error);
                    }

                    return attachResult;
                }
                finally
                {
                    if (File.Exists(tempPolicyFile))
                    {
                        File.Delete(tempPolicyFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while setting policy for access key: {AccessKey} on bucket: {BucketName}",
                    accessKey, bucketName);
                return new MinioCommandResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        #region MinIO Client Helper Methods

        /// <summary>
        /// Ensures the MinIO client is initialized by creating/setting the alias.
        /// The 'mc alias set' command creates the alias if it doesn't exist, or updates it if it does.
        /// This must be called before any MinIO operations.
        /// </summary>
        private async Task<bool> EnsureMinioClientInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (!_isMinioClientInitialized)
            {
                var command = $"mc alias set {_minioSettings.Alias} {_minioSettings.Url} {_minioSettings.AccessKey} {_minioSettings.SecretKey}";
                var result = await ExecuteMinioCommandAsync(command, cancellationToken);

                if (result.Success)
                {
                    _isMinioClientInitialized = true;
                    _logger.LogInformation("MinIO MC client initialized successfully with alias: {Alias}", _minioSettings.Alias);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to initialize MinIO MC client: {Error}", result.Error);
                    return false;
                }
            }
            return true;
        }

        private async Task<MinioCommandResult> ExecuteMinioCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            var result = new MinioCommandResult();

            try
            {
                _logger.LogDebug("Executing MinIO command: {Command}", command);

                if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                {
                    var errorMsg = "MinIO management service is only supported in Linux/macOS environments (Docker container). " +
                                   "Current OS: " + Environment.OSVersion.Platform;
                    _logger.LogError(errorMsg);
                    return new MinioCommandResult
                    {
                        Success = false,
                        Error = errorMsg,
                        ExitCode = -1
                    };
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process();
                process.StartInfo = processStartInfo;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                result.ExitCode = process.ExitCode;
                result.Output = outputBuilder.ToString().Trim();
                result.Error = errorBuilder.ToString().Trim();
                result.Success = process.ExitCode == 0;

                if (result.Success)
                {
                    _logger.LogDebug("MinIO command executed successfully: {Output}", result.Output);
                }
                else
                {
                    _logger.LogWarning("MinIO command failed with exit code {ExitCode}: {Error}", result.ExitCode, result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while executing MinIO command: {Command}", command);
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Generate a bucket policy JSON for read/write access
        /// </summary>
        private static string GenerateBucketPolicy(string bucketName)
        {
            return $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""s3:ListBucket"",
                            ""s3:GetBucketLocation"",
                            ""s3:ListBucketMultipartUploads""
                        ],
                        ""Resource"": [
                            ""arn:aws:s3:::{bucketName}""
                        ]
                    }},
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""s3:GetObject"",
                            ""s3:PutObject"",
                            ""s3:DeleteObject"",
                            ""s3:ListMultipartUploadParts"",
                            ""s3:AbortMultipartUpload""
                        ],
                        ""Resource"": [
                            ""arn:aws:s3:::{bucketName}/*""
                        ]
                    }}
                ]
            }}";
        }

        #endregion
    }
}
