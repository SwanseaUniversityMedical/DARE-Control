using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Aws4RequestSigner;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Minio.Exceptions;
using Newtonsoft.Json;
using Serilog;
using System.Net;

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

        public async Task<bool> UploadFileAsync(MinioSettings minioSettings, IFormFile? filePath, string bucketName = "", string objectName = "")
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);
            if (filePath != null)
            {
                using (var stream = new MemoryStream())
                {
                    await filePath.CopyToAsync(stream);
                    var uploadRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = filePath.FileName,
                        InputStream = stream,
                        ContentType = filePath.ContentType
                    };


                    var response = await amazonS3Client.PutObjectAsync(uploadRequest);
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
            }
            return true;
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
        public async Task<bool> FetchAndStoreObject(string url, MinioSettings minioSettings, string bucketName, string key)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var contentBytes = await response.Content.ReadAsByteArrayAsync();

                var amazonS3Client = GenerateAmazonS3Client(minioSettings);

                using (var transferUtility = new TransferUtility(amazonS3Client))
                {
                    await transferUtility.UploadAsync(new MemoryStream(contentBytes), bucketName, key);
                }
            }

            return true;
        }

        public async Task<bool> RabbitExternalObject(FetchFileMQ msgBytes)
        {
            if (msgBytes == null)
            {
                return false;
            }
            else
            {
                await FetchAndStoreObject(msgBytes.Url, _minioSettings, msgBytes.BucketName, msgBytes.Key);
                return true;
            }
        }

        public async Task<bool> RabbitExternalObject(string msgBytes)
        {
            var FileInfo = JsonConvert.DeserializeObject<FetchFileMQ>(msgBytes);
            if (FileInfo == null)
            {
                return false;
            }
            else
            {
                await FetchAndStoreObject(FileInfo.Url, _minioSettings, FileInfo.BucketName, FileInfo.Key);
                return true;
            }
        }

        public async Task<bool> CreateBucketPolicy(string bucketName)
        {

            var signer = new AWS4RequestSigner(_minioSettings.AccessKey, _minioSettings.SecretKey);

            var content = new StringContent("{\r\n    \"Version\": \"2012-10-17\",\r\n    \"Statement\": [\r\n        {\r\n            \"Effect\": \"Allow\",\r\n            \"Action\": [\r\n                \"s3:List*\",\r\n                \"s3:ListBucket\",\r\n                \"s3:PutObject\",\r\n                \"s3:DeleteObject\",\r\n                \"s3:GetBucketLocation\"\r\n            ],\r\n            \"Resource\": [\r\n                \"arn:aws:s3:::" + bucketName + "\"\r\n            ]\r\n        }\r\n    ]\r\n}", null, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("http://" + _minioSettings.Url + "/minio/admin/v3/add-canned-policy?name=" + bucketName + "_policy"),
                Content = content
            };

            request = await signer.Sign(request, _minioSettings.AWSService, _minioSettings.AWSRegion);
            var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> CopyObject(MinioSettings minioSettings, string sourceBucketName, string destinationBucketName, string sourceObjectKey, string destinationObjectKey)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            var request = new CopyObjectRequest
            {
                SourceBucket = sourceBucketName,
                DestinationBucket = destinationBucketName,
                SourceKey = sourceObjectKey,
                DestinationKey = destinationObjectKey
            };

            var result = await amazonS3Client.CopyObjectAsync(request);

            return true;
        }

        public async Task<string> ShareMinioObject(MinioSettings minioSettings, string bucketName, string objectKey)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            var expiration = DateTime.Now.AddHours(1);


            var url = amazonS3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = expiration,
            });

            return url;
        }
        public async Task<bool> FolderExists(MinioSettings minioSettings, string bucketName, string folderName)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);
            try
            {
                var listReqest = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = folderName
                };

                var listResponse = await amazonS3Client.ListObjectsAsync(listReqest);
                if (!listResponse.S3Objects.Any())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

                return true;
            }

        }
        public async Task<bool> CreateFolder(MinioSettings minioSettings, string bucketName, string folderName)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = folderName + "/",
                    ContentBody = string.Empty
                };

                var putResponse = await amazonS3Client.PutObjectAsync(putRequest);

                if (putResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
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
