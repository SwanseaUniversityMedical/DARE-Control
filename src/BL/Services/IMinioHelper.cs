﻿using Amazon.S3.Model;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BL.Services
{
    public interface IMinioHelper
    {
        Task<bool> CheckBucketExists(string bucketName = "");
        Task<bool> CreateBucket(string bucketName = "");
        Task<bool> UploadFileAsync(IFormFile? filePath, string bucketName = "", string objectName = "");
        Task<bool> DownloadFileAsync(string bucketName = "", string objectName = "");
        Task<bool> CheckObjectExists(string bucketName, string objectKey);
        Task<bool> FetchAndStoreObject(string url, string bucketName, string key);
        Task<bool> RabbitExternalObject(MQFetchFile msgBytes);
        Task<bool> CreateBucketPolicy(string bucketName);
        Task<bool> CopyObjectToDestination(string destinationBucketName, string destinationObjectKey, GetObjectResponse response);
        Task<GetObjectResponse> GetCopyObject(string sourceBucketName, string sourceObjectKey);
        Task<string> ShareMinioObject(string bucketName, string objectKey);
        Task<bool> FolderExists(string bucketName, string folderName);
        Task<bool> CreateFolder(string bucketName, string folderName);

        Task<ListObjectsV2Response> GetFilesInBucket(string bucketName, string prefix = "");


        Task<bool> SetPublicPolicy(string bucketName);

        Task<bool> BucketPolicySetPublic(string bucketName);

        Task DeleteObject(string bucketName, string objectKey);
    }
}
