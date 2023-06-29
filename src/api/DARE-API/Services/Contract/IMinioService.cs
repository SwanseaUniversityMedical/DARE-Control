using Amazon.S3.Model;
using DARE_API.Models;

namespace DARE_API.Services.Contract
{
    public interface IMinioService
    {
        Task<bool> CreateBucket(MinioSettings minioSettings, string bucketName = "");
        Task<bool> CheckBucketExists(MinioSettings minioSettings, string bucketName = "");
        Task<List<S3Object>> ListObjectsInBucket(MinioSettings minioSettings, string bucketName = "", string filePathPrefix = "");
        Task<bool> DeleteObjectAsync(MinioSettings minioSettings, string bucketName, string fileLocation);

        Task<bool> CopyFile(MinioSettings minioSettings, string sourceBucketName, string sourceLocation, string destBucketName, string destLocation);
        Task<bool> CopyObject(MinioSettings minioSettings, string sourceBucketName, string sourceLocation, string destBucketName, string destLocation);
        Task<bool> CopyObjectParts(MinioSettings minioSettings, string sourceBucketName, string sourceLocation, string destBucketName, string destLocation);
        Task<bool> CopyObjectPartsServer(MinioSettings sourceMinioSettings, string sourceBucketName, string sourceLocation, MinioSettings destMinioSettings, string destBucketName, string destLocation);

        Task<string> GetFileSignedUrl(string objectKey, MinioSettings _minioSettings, string bucketName = "");
    }
}
