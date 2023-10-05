﻿using BL.Models.ViewModels;

namespace BL.Services
{
    public interface IMinioHelper
    {
        Task<bool> CheckBucketExists(MinioSettings minioSettings, string bucketName = "");
        Task<bool> CreateBucket(MinioSettings minioSettings, string bucketName = "");
        Task<bool> UploadFileAsync(MinioSettings minioSettings, string bucketName = "", string objectName="", string filePath = "");
        Task<bool> DownloadFileAsync(MinioSettings minioSettings, string bucketName = "", string objectName = "");
        Task<bool> CheckObjectExists(MinioSettings minioSettings, string bucketName, string objectKey);
        Task<bool> FetchAndStoreObject(string url, MinioSettings minioSettings, string bucketName, string key);
        Task<bool> RabbitExternalObject(FetchFileMQ msgBytes);
        Task<bool> CreateBucketPolicy(string bucketName);

    }
}
