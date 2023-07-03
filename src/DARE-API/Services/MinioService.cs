using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BL.Models;
using DARE_API.Services.Contract;
using Minio.Exceptions;
using Serilog;
using System.Net;

namespace DARE_API.Services
{
    public class MinioService : IMinioService
    {
        public async Task<bool> CheckBucketExists(MinioSettings minioSettings, string bucketName = "")
        {
            if (string.IsNullOrEmpty(bucketName)) { bucketName = minioSettings.BucketName; }

            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            var buckets = await amazonS3Client.ListBucketsAsync();
            return buckets.Buckets.Any(x => x.BucketName.Equals(bucketName));
        }

        public async Task<bool> CopyFile(MinioSettings minioSettings, string sourceBucketName, string sourceLocation, string destBucketName, string destLocation)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);
            //get object metadata from source
            GetObjectMetadataRequest metadataRequest = new GetObjectMetadataRequest() { BucketName = sourceBucketName, Key = sourceLocation };
            var metadataResult = await amazonS3Client.GetObjectMetadataAsync(metadataRequest);
            long fileSize = metadataResult.ContentLength;
            var sizeGB = fileSize * (9.32 * Math.Pow(10, -10));

            bool moveRes;
            if (sizeGB > 5)
            {
                moveRes = await CopyObjectParts(minioSettings, sourceBucketName, sourceLocation, destBucketName, destLocation);
            }
            else
            {
                moveRes = await CopyObject(minioSettings, sourceBucketName, sourceLocation, destBucketName, destLocation);
            }

            if (!moveRes)
            {
                Log.Error("MoveFile: Failed to moved file {file} to destination folder.", sourceLocation);
                return false;
            }
            return true;
        }

        public async Task<bool> CopyObject(MinioSettings minioSettings, string sourceBucketName, string sourceLocation, string destBucketName, string destLocation)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            try
            {
                Log.Information("CopyObject: Prepare to send copy file {fileSource} from bucket {bucketSource} to bucket {bucketDest} and destination {fileDest}.", sourceLocation, sourceBucketName, destBucketName, destLocation);
                CopyObjectRequest request = new CopyObjectRequest
                {
                    SourceBucket = sourceBucketName,
                    SourceKey = sourceLocation,
                    DestinationBucket = destBucketName,
                    DestinationKey = destLocation
                };
                var copyResult = await amazonS3Client.CopyObjectAsync(request);
                if (copyResult.HttpStatusCode != HttpStatusCode.OK)
                {
                    Log.Error("CopyObject: Failed to Copy object in Minio/S3.");
                    return false;
                }
            }
            catch (MinioException me)
            {
                Log.Error("CopyObject: Failed to Copy object in Minio/S3 due to Minio Error: {error}", me.Message);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("CopyObject: Failed to Copy object in Minio/S3 due to Error: {error}", ex.Message);
                return false;
            }
            Log.Information("CopyObject: successfully coppied file {fileSource} from bucket {bucketSource} to bucket {bucketDest} and destination {fileDest}.", sourceLocation, sourceBucketName, destBucketName, destLocation);
            return true;
        }

        public async Task<bool> CopyObjectParts(MinioSettings minioSettings, string sourceBucketName, string sourceLocation, string destBucketName, string destLocation)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);
            try
            {
                Log.Information("CopyObjectParts: Prepare to send copy file {fileSource} from bucket {bucketSource} to bucket {bucketDest} and destination {fileDest}.", sourceLocation, sourceBucketName, destBucketName, destLocation);
                var initalResponse = await amazonS3Client.InitiateMultipartUploadAsync(destBucketName, destLocation);

                GetObjectMetadataRequest metadataRequest = new GetObjectMetadataRequest() { BucketName = sourceBucketName, Key = sourceLocation };
                var metadataResult = await amazonS3Client.GetObjectMetadataAsync(metadataRequest);
                long objectSize = metadataResult.ContentLength;

                //calculate part size starting with 5 Megabytes chunks
                long partSize = CalcularePartSize(objectSize, 5 * 1024 * 1024);
                long bytePosition = 0;
                int partNum = 0;
                var PartETags = new List<PartETag>();
                var numParts = objectSize / partSize;

                Log.Information("CopyObjectPartsThe file to multipart upload to S3 is {objSize} in size, will transfer in chunks of {chunkSize} with {numParts} parts", objectSize, partSize, numParts);

                while (bytePosition < objectSize)
                {
                    // The last part might be smaller than partSize, so check to make sure
                    // that lastByte isn't beyond the end of the object.
                    long lastByte = Math.Min(bytePosition + partSize - 1, objectSize - 1);
                    // Copy this part.
                    CopyPartRequest copyPart = new CopyPartRequest()
                    {
                        SourceBucket = sourceBucketName,
                        SourceKey = sourceLocation,
                        DestinationBucket = destBucketName,
                        DestinationKey = destLocation,
                        UploadId = initalResponse.UploadId,
                        FirstByte = bytePosition,
                        LastByte = lastByte,
                        PartNumber = partNum++
                    };
                    var res = await amazonS3Client.CopyPartAsync(copyPart);
                    PartETags.Add(new PartETag() { ETag = res.ETag, PartNumber = res.PartNumber });
                    bytePosition += partSize;
                }

                CompleteMultipartUploadRequest complete = new CompleteMultipartUploadRequest()
                {
                    BucketName = destBucketName,
                    Key = destLocation,
                    UploadId = initalResponse.UploadId,
                    PartETags = PartETags
                };
                var partResponse = await amazonS3Client.CompleteMultipartUploadAsync(complete);

                if (partResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    Log.Error("CopyObject: Failed to Copy object in Minio/S3.");
                    return false;
                }
            }
            catch (MinioException me)
            {
                Log.Error("CopyObjectParts: Failed to delete object in Minio due to Minio Error: {error}", me.Message);
                return false;
            }
            catch (AmazonS3Exception ae)
            {
                Log.Error("CopyObjectParts: Failed to Copy object in S3 due to Error: {error}", ae.Message);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("CopyObjectParts: Failed to delete object in Minio/S3 due to Error: {error}", ex.Message);
                return false;
            }
            Log.Information("CopyObject: successfully coppied file {fileSource} from bucket {bucketSource} to bucket {bucketDest} and destination {fileDest}.", sourceLocation, sourceBucketName, destBucketName, destLocation);
            return true;
        }

        public async Task<bool> CopyObjectPartsServer(MinioSettings sourceMinioSettings, string sourceBucketName, string sourceLocation, MinioSettings destMinioSettings, string destBucketName, string destLocation)
        {
            var amazonS3ClientSource = GenerateAmazonS3Client(sourceMinioSettings);
            var amazonS3ClientDest = GenerateAmazonS3Client(destMinioSettings);

            try
            {
                //initiate multipart upload for the destination of the file
                var initalResponse = await amazonS3ClientDest.InitiateMultipartUploadAsync(destBucketName, destLocation);

                //get object metadata from source
                GetObjectMetadataRequest metadataRequest = new GetObjectMetadataRequest() { BucketName = sourceBucketName, Key = sourceLocation };
                var metadataResult = await amazonS3ClientSource.GetObjectMetadataAsync(metadataRequest);
                long objectSize = metadataResult.ContentLength;

                //calculate part size starting with 5 Megabytes chunks
                long partSize = CalcularePartSize(objectSize, 5 * 1024 * 1024);
                long bytePosition = 0;
                int partNum = 0;
                var PartETags = new List<PartETag>();
                var numParts = objectSize / partSize;

                Log.Information("CopyObjectPartsServer: The file to multipart upload to S3 is {objSize} in size, will transfer in chunks of {chunkSize} with {numParts} parts", objectSize, partSize, numParts);

                //loop over the object in chunks until complete
                while (bytePosition < objectSize)
                {
                    //calculate the last bite depending on what is smalled (i.e. the last chunk could be less than the size of the partSize)
                    long lastByte = Math.Min(bytePosition + partSize, objectSize);

                    //get object from source only selecting a chunk of it
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = sourceBucketName,
                        Key = sourceLocation,
                        ByteRange = new ByteRange(bytePosition, lastByte),

                    };
                    var getObjectResponse = await amazonS3ClientSource.GetObjectAsync(request);
                    var stream = getObjectResponse.ResponseStream;

                    //await getObjectResponse.WriteResponseStreamToFileAsync("C:\\Users\\Llyr\\Desktop\\minioTest\\part", false, System.Threading.CancellationToken.None);
                    using (var memStream = new MemoryStream())
                    {
                        //need to convert to memory stream as the upload part requires a seek which the original stream does not contain
                        await stream.CopyToAsync(memStream);
                        memStream.Position = 0; // this was the fix!!!!!!!!!
                        Log.Information($"part no. {partNum}. partsize {lastByte - bytePosition}. bytePosition {bytePosition} of {objectSize}.");

                        // Copy this part.
                        UploadPartRequest uploadPartRequest = new UploadPartRequest()
                        {
                            BucketName = destBucketName,
                            Key = destLocation,
                            UploadId = initalResponse.UploadId,
                            PartNumber = partNum++,
                            PartSize = partSize,
                            //FilePath = "C:\\Users\\Llyr\\Desktop\\minioTest\\part",
                            InputStream = memStream
                        };

                        var res = await amazonS3ClientDest.UploadPartAsync(uploadPartRequest);
                        //add the eTag and partNumber to list to be used in complete multipart
                        PartETags.Add(new PartETag() { ETag = res.ETag, PartNumber = res.PartNumber });
                        bytePosition += partSize;
                    }
                }

                CompleteMultipartUploadRequest complete = new CompleteMultipartUploadRequest()
                {
                    BucketName = destBucketName,
                    Key = destLocation,
                    UploadId = initalResponse.UploadId,
                    PartETags = PartETags
                };
                var partResponse = await amazonS3ClientDest.CompleteMultipartUploadAsync(complete);

                if (partResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    Log.Error("CopyObjectPartsServer: Failed to Copy object in Minio/S3.");
                    return false;
                }
            }
            catch (MinioException me)
            {
                Log.Error("CopyObjectPartsServer: Failed to delete object in Minio due to Minio Error: {error}", me.Message);
                return false;
            }
            catch (AmazonS3Exception ae)
            {
                Log.Error("CopyObjectPartsServer: Failed to Copy object in S3 due to Error: {error}", ae.Message);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("CopyObjectPartsServer: Failed to delete object in Minio/S3 due to Error: {error}", ex.Message);
                return false;
            }
            return true;
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

        public async Task<bool> DeleteObjectAsync(MinioSettings minioSettings, string bucketName, string fileLocation)
        {
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            try
            {
                DeleteObjectRequest request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileLocation.Replace($"{bucketName}/", ""),
                };
                var deleteResult = await amazonS3Client.DeleteObjectAsync(request);
                if (!deleteResult.HttpStatusCode.Equals(HttpStatusCode.NoContent))
                {
                    Log.Error("DeleteObjectAsync: Failed to delete object'{fileLocation}' in Minio/S3.", fileLocation);
                    return false;
                }
            }
            catch (MinioException me)
            {
                Log.Error("DeleteObjectAsync: Failed to delete object in Minio due to Minio Error: {error}", me.Message);
                return false;
            }
            catch (AmazonS3Exception ae)
            {
                Log.Error("DeleteObjectAsync: Failed to Copy object in S3 due to Error: {error}", ae.Message);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("DeleteObjectAsync: Failed to delete object in Minio/S3 due to Error: {error}", ex.Message);
                return false;
            }
            return true;
        }

        public async Task<string> GetFileSignedUrl(string objectKey, MinioSettings _minioSettings, string bucketName = "")
        {
            if (string.IsNullOrEmpty(bucketName)) { bucketName = _minioSettings.BucketName; }
            var amazonS3Client = GenerateAmazonS3Client(_minioSettings);

            GetPreSignedUrlRequest requestObj = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            string urlString = amazonS3Client.GetPreSignedURL(requestObj);

            return urlString;
        }

        public async Task<List<S3Object>> ListObjectsInBucket(MinioSettings minioSettings, string bucketName = "", string filePathPrefix = "")
        {
            if (string.IsNullOrEmpty(bucketName)) { bucketName = minioSettings.BucketName; }
            var amazonS3Client = GenerateAmazonS3Client(minioSettings);

            ListObjectsRequest listObjectsRequest = new ListObjectsRequest()
            {
                BucketName = bucketName,
                Prefix = filePathPrefix
            };

            var bucketObjects = await amazonS3Client.ListObjectsAsync(listObjectsRequest);

            return bucketObjects.S3Objects;
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
