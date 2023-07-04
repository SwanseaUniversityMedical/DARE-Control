﻿using Amazon;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using BL.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Minio.Exceptions;
using Serilog;
using System.Net;
using System.Security.AccessControl;

namespace BL.Services
{
    public class MinioHelper : IMinioHelper
    {
        private readonly MinioSettings _minioSettings;
        public MinioHelper(MinioSettings minioSettings)
        {
            _minioSettings = minioSettings;
        }
        public async Task<bool> CheckBucketExists(MinioSettings minioSettings, string bucketName = "")
        {
            if (string.IsNullOrEmpty(bucketName)) { bucketName = minioSettings.BucketName; }

            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            var buckets = await amazonS3Client.ListBucketsAsync();
            return buckets.Buckets.Any(x => x.BucketName.Equals(bucketName));
        }
        public async Task<bool> CreateBucket(MinioSettings minioSettings, string bucketName = "")
        {
            if (string.IsNullOrEmpty(bucketName)) { bucketName = minioSettings.BucketName; }

            if (!string.IsNullOrEmpty(bucketName))
            {
                var amazonS3Client = GenerateAmazonS3Client(minioSettings);

                try
                {
                    // Create bucket if it doesn't exist.
                    bool found = await CheckBucketExists(minioSettings, bucketName);

                    if (found)
                    {
                        Log.Information("{bucketName} already exists.", bucketName);
                        return true;
                    }
                    else
                    {
                        await amazonS3Client.PutBucketAsync(bucketName);
                        Log.Information("{bucketName} created successfully.", bucketName);
                        return true;
                    }
                }
                catch (MinioException e)
                {
                    Log.Warning("Create bucket: {bucketName}, failed due to Minio exception: {message}", bucketName, e.Message);
                }
                catch (Exception ex)
                {
                    Log.Warning("Create bucket: {bucketName}, failed due to Exception: {message}", bucketName, ex.Message);
                }
            }
            else
            {
                Log.Warning("Cannot create bucket as bucket name is null or empty.");
            }
            return false;
        }

        public async Task<bool> UploadFileAsync(MinioSettings minioSettings, string bucketName = "", string objectName = "", string filePath = "")
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                FilePath = filePath,
            };

            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            var response = await amazonS3Client.PutObjectAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Log.Warning($"Successfully uploaded {objectName} to {bucketName}.");
                return true;
            }
            else
            {
                Log.Warning($"Could not upload {objectName} to {bucketName}.");
                return false;
            }
        }

        public async Task<bool> DownloadFileAsync(MinioSettings minioSettings, string bucketName = "", string objectName = "")
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
            };

            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            var objectExists = await CheckObjectExists(_minioSettings, request.BucketName, request.Key);

            if (objectExists)
            {
                var response = await amazonS3Client.GetObjectAsync(request);

                using (var responseStream = response.ResponseStream)
                {
                    string saveFilePath = "C:/Path/To/Save/File.txt";

                    using (var fileStream = new FileStream(saveFilePath, FileMode.Create))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }
                }

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Log.Warning($"Successfully uploaded {objectName} to {bucketName}.");
                    return true;
                }
                else
                {
                    Log.Warning($"Could not upload {objectName} to {bucketName}.");
                    return false;
                }
            }
            else
            {
                return false;
            }

            
        }

        public async Task<bool> CheckObjectExists(MinioSettings minioSettings, string bucketName, string objectKey)
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            try
            {
                await amazonS3Client.GetObjectMetadataAsync(request);
                Log.Warning($"{request.Key} Exists on {bucketName}.");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.Warning($"{request.Key} Not Exists on {bucketName}.");
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        #region PrivateHelpers
        private AmazonS3Config GenerateAmazonS3Config(MinioSettings minioSettings)
        {
            return new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1, // MUST set this before setting ServiceURL and it should match the `MINIO_REGION` environment variable.
                ServiceURL = $"http://{minioSettings.Url}", // replace http://localhost:9000 with URL of your MinIO server
                ForcePathStyle = true, // MUST be true to work correctly with MinIO server
            };
        }

        private AmazonS3Client GenerateAmazonS3Client(MinioSettings minioSettings)
        {
            var config = GenerateAmazonS3Config(minioSettings);
            return new AmazonS3Client(minioSettings.AccessKey, minioSettings.SecretKey, config);
        }

        private long CalcularePartSize(long objectSize, long partSize)
        {
            var numParts = objectSize / partSize;
            //s3 can only handle 10000 parts so if to large then increase size of part
            if (numParts >= 10000)
            {
                partSize *= 2;
                return CalcularePartSize(objectSize, partSize);
            }
            else
            {
                return partSize;
            }
        }
        #endregion
    }
}
