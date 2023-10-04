using BL.Models;
using BL.Models.APISimpleTypeReturns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data_Egress_API.Repositories.DbContexts;
using Castle.Components.DictionaryAdapter.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using BL.Models.Enums;
using EasyNetQ.Management.Client.Model;
using System.IO;
using BL.Models.ViewModels;
using Amazon.S3.Model;
using Amazon.S3;
using Amazon;
using System.Net;
using System.Collections.Generic;

namespace Data_Egress_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataEgressController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly MinioSettings _minioSettings;
        public DataEgressController(ApplicationDbContext repository, MinioSettings minioSettings)
        {
            _DbContext = repository;
                 _minioSettings = minioSettings;
        }
        [HttpPost("AddNewDataEgress")]
        public async Task<BoolReturn> AddNewDataEgress(int submissionId, List<SubmissionFile> files)
        {
            var existingDataFiles = _DbContext.DataEgressFiles            
                .FirstOrDefault(d => d.Id == submissionId);

            var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            if (existingDataFiles != null)
            {
                foreach (var file in files)
                {
                   
                    var dataFile = new DataFiles()
                    {
                        Name = file.Name,
                        TreBucketFullPath = file.TreBucketFullPath,
                        SubmisionBucketFullPath = file.SubmisionBucketFullPath,
                        Status = file.Status,
                        Description = file.Description,
                        LastUpdate = DateTime.Now.ToUniversalTime(),
                        Reviewer = approvedBy,
                        SubmissionId = file.Id
                    };
                    _DbContext.DataEgressFiles.Add(dataFile);
                
                }
                await _DbContext.SaveChangesAsync();
                return new BoolReturn() { Result = true };
            }
            else
            {
                return new BoolReturn() { Result = false };
            }

        }


        [HttpGet("GetAllFiles")]
        public List<DataFiles> GetAllFiles()
        {
            try
            {
                var allFiles = _DbContext.DataEgressFiles.ToList();
          
                Log.Information("{Function} Files retrieved successfully", "GetAllFiles");
                
                return allFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllFiles");
                throw;
            }
        }

          [HttpGet("GetFilesBySubmissionId")]
        public List<DataFiles> GetFilesBySubmissionId(int id)
        {
            try
            {
                var returned = _DbContext.DataEgressFiles.Where(x => x.SubmissionId == id).ToList();
                if (returned == null)
                {
                    return null;
                }

                Log.Information("{Function} Files retrieved successfully", "GetFilesBySubmissionId");
                return returned.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetProject");
                throw;
            }


        }

        [HttpGet("GetAllUnprocessedFiles")]
        public List<DataFiles> GetAllUnprocessedFiles()
        {
            try
            {
                var allUnprocessedFiles = _DbContext.DataEgressFiles.Where(x => x.Status != FileStatus.Approved).ToList();

                Log.Information("{Function} Files retrieved successfully", "GetAllUnprocessedFiles");
                return allUnprocessedFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "allUnprocessedFiles");
                throw;
            }
        }

        [HttpPost("UpdateFileData")]

        public async Task<List<DataFiles>> UpdateFileData(List<DataFiles> dataFiles)
        {
            var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            if (string.IsNullOrWhiteSpace(approvedBy))
            {
                approvedBy = "[Unknown]";
            }
            var resultList = new List<DataFiles>();
            var approvedDate = DateTime.Now.ToUniversalTime();
            foreach (var file in dataFiles)
            {
                var dbFile = _DbContext.DataEgressFiles.First(x => x.Id == file.Id);

                if (file.Status != dbFile.Status)
                {
                    dbFile.Status = file.Status;
                    dbFile.Reviewer = approvedBy;
                    dbFile.LastUpdate = approvedDate;
                }
                resultList.Add(dbFile);
                Log.Information("{Function}:", "UpdateFileData", "FileData Status:" + file.Status.ToString(), "ApprovedBy:" + approvedBy);
            }
            await _DbContext.SaveChangesAsync();
            return resultList;
        }
          
        [HttpGet("DownloadFile")]
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
        [HttpGet("CheckObjectExists")]
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
        #endregion
    }

}

