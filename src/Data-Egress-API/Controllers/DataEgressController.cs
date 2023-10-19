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
using BL.Services;
using System.Net.Mime;
using Data_Egress_API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Sentry.PlatformAbstractions;

namespace Data_Egress_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    
    public class DataEgressController : ControllerBase
    {
        private readonly ApplicationDbContext _DbContext;
        
        private readonly ITreClientWithoutTokenHelper _treClientHelper;
        private readonly IMinioHelper _minioHelper;
        public DataEgressController(ApplicationDbContext repository, ITreClientWithoutTokenHelper treClientHelper, IMinioHelper minioHelper)            
        {
            _DbContext = repository;
            _minioHelper = minioHelper;
            _treClientHelper = treClientHelper;
            
        }

        [Authorize(Roles = "data-egress-admin,dare-tre-admin")]
        [HttpPost("AddNewDataEgress")]
        public async Task<BoolReturn> AddNewDataEgress(EgressSubmission submission)
        {
            try
            {
                var existingDataFiles = _DbContext.EgressSubmissions
                .FirstOrDefault(d => d.SubmissionId == submission.SubmissionId);

                
                if (existingDataFiles == null)
                {
                    _DbContext.EgressSubmissions.Add(submission);
                    foreach (var submissionFile in submission.Files)
                    {
                        submissionFile.Status = FileStatus.Undecided;
                    }
                    
                    await _DbContext.SaveChangesAsync();
                    return new BoolReturn() { Result = true };
                }
                else
                {
                    return new BoolReturn() { Result = false };
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddNewDataEgress");
                throw;
            }
        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("GetAllEgresses")]
        public List<EgressSubmission> GetAllEgresses()
        {
            try
            {
                var allFiles = _DbContext.EgressSubmissions.ToList();

                Log.Information("{Function} Files retrieved successfully", "GetAllEgresses");

                return allFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllEgresses");
                throw;
            }
        }
        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("GetEgress")]
        public EgressSubmission GetEgress(int id)
        {
            try
            {
                var returned = _DbContext.EgressSubmissions.First(x => x.Id == id);
                

                Log.Information("{Function} Files retrieved successfully", "GetEgress");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetEgress");
                throw;
            }


        }
        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("GetEgressFile")]
        public EgressFile GetEgressFile(int id)
        {
            try
            {
                var returned = _DbContext.EgressFiles.First(x => x.Id == id);


                Log.Information("{Function} File retrieved successfully", "GetEgressFile");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetEgressFile");
                throw;
            }


        }
        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("GetAllUnprocessedEgresses")]
        public List<EgressSubmission> GetAllUnprocessedEgresses()
        {
            try
            {
                var allUnprocessedFiles = _DbContext.EgressSubmissions.Where(x => x.Status == EgressStatus.NotCompleted).ToList();

                Log.Information("{Function} Files retrieved successfully", "GetAllUnprocessedEgresses");
                return allUnprocessedFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllUnprocessedEgresses");
                throw;
            }
        }
        [Authorize(Roles = "data-egress-admin")]
        [HttpPost("CompleteEgress")]
        public async Task<EgressSubmission> CompleteEgressAsync([FromBody] EgressSubmission egress)
        {
            try
            {
                var dbegress = _DbContext.EgressSubmissions.First(x => x.Id == egress.Id);
                if (dbegress.Status != EgressStatus.NotCompleted)
                {
                    throw new Exception("Egress has already been completed");
                }
                var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(approvedBy))
                {
                    approvedBy = "[Unknown]";
                }
                var approvedDate = DateTime.Now.ToUniversalTime();
                if (egress.Files.Any(x => x.Status == FileStatus.Undecided))
                {
                    throw new Exception("Not all files reviewed");
                }
                else if (egress.Files.All(x => x.Status == FileStatus.Rejected))
                {
                    dbegress.Status = EgressStatus.FullyRejected;
                }
                else if (egress.Files.All(x => x.Status == FileStatus.Approved))
                {
                    dbegress.Status = EgressStatus.FullyApproved;
                }
                else
                {
                    dbegress.Status = EgressStatus.PartiallyApproved;
                }
                dbegress.Reviewer = approvedBy;
                dbegress.Completed = approvedDate;

                var backtotre = new EgressReview()
                {
                    SubId = dbegress.SubmissionId,
                    FileResults = new List<EgressResult>()
                };
                foreach (var file in egress.Files)
                {
                    var dbegressfile = dbegress.Files.First(x => x.Id == file.Id);
                    dbegressfile.Status = file.Status;
                    backtotre.FileResults.Add(new EgressResult()
                    {
                        FileName = dbegressfile.Name,
                        Approved = dbegressfile.Status == FileStatus.Approved,
                    });
                    var egfile = dbegress.Files.First(x => x.Id == file.Id);
                    egfile.Status = file.Status;
                    egfile.LastUpdate = approvedDate;
                    egfile.Reviewer = approvedBy;

                }

                var result =
                    await _treClientHelper.CallAPI<EgressReview, Submission>("/api/Submission/EgressResults", backtotre);
                await _DbContext.SaveChangesAsync();

                
                
               
                




                Log.Information("{Function} Egress Completed for Submission {SubId}", "CompleteEgress", dbegress.SubmissionId);
                return dbegress;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "CompleteEgress");
                throw;
            }

        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpPost("UpdateFileData")]
        public EgressFile UpdateFileData(int fileId, FileStatus status)
        {
            try
            {
                var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(approvedBy))
                {
                    approvedBy = "[Unknown]";
                }
                var approvedDate = DateTime.Now.ToUniversalTime();

                var returned = _DbContext.EgressFiles.First(x => x.Id == fileId);
                
                    returned.Status = status;
                    returned.Reviewer = approvedBy;
                    returned.LastUpdate = approvedDate;
                
                
                _DbContext.SaveChangesAsync();
                Log.Information("{Function} File updated", "UpdateFileData");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "UpdateFileData");
                throw;
            }

        }

        //[HttpPost("MarkEgressAsComplete")]
        //public async Task<EgressSubmission> MarkEgressAsCompleteAsync(int id)
        //{
        //    try
        //    {
        //        var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).FirstOrDefault();
        //        if (string.IsNullOrWhiteSpace(approvedBy))
        //        {
        //            approvedBy = "[Unknown]";
        //        }
        //        var approvedDate = DateTime.Now.ToUniversalTime();

        //        var returned = _DbContext.EgressSubmissions.First(x => x.Id == id);
        //        if (returned.Files.Any(x => x.Status == FileStatus.Undecided))
        //        {
        //            throw new Exception("Not all files reviewed");
        //        }else if (returned.Files.All(x => x.Status == FileStatus.Rejected))
        //        {
        //            returned.Status = EgressStatus.FullyRejected;
        //        }
        //        else if (returned.Files.All(x => x.Status == FileStatus.Approved))
        //        {
        //            returned.Status = EgressStatus.FullyApproved;
        //        }
        //        else
        //        {
        //            returned.Status = EgressStatus.PartiallyApproved;
        //        }

        //        returned.Reviewer = approvedBy;
        //        returned.Completed = approvedDate;


        //        await _DbContext.SaveChangesAsync();
        //        var backtotre = new EgressReview()
        //        {
        //            subId = returned.SubmissionId,
        //            fileResults = new List<EgressResult>()
        //        };
        //        foreach (var file in returned.Files)
        //        {
        //            backtotre.fileResults.Add(new EgressResult()
        //            {
        //                fileName = file.Name,
        //                approved = file.Status == FileStatus.Approved,
        //            });
        //        }

        //        var result =
        //            await _treClientHelper.CallAPI<EgressReview, Submission>("/api/Submission/EgressReview", backtotre);
        //        return returned;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "{Function} Crashed", "MarkEgressAsComplete");
        //        throw;
        //    }

        //}

        //[HttpGet("DataOut")]
        //public async Task<BoolReturn> DataOutApproval(int submissionId)
        //{ 
        //    try
        //    { 
        //    var returned = _DbContext.DataEgressFiles.Where(x => x.SubmissionId == submissionId).ToList();

        //    var paramlist = new Dictionary<string, string>();
        //    paramlist.Add("submissionId", submissionId.ToString());
        //        var submission = await _treClientHelper.CallAPI<List<SubmissionFile>, Submission>("/api/SendFileResultsToHUTCH/Submission/", returned,
        //                paramlist).Result;
        //        if (submission!= null)
        //    {
        //        return new BoolReturn() { Result = true };
        //    }
        //    else
        //    {
        //        return new BoolReturn() { Result = false };
        //    }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "{Function} Crashed", "DataOutApproval");
        //        throw;
        //    }
        //}

        public static string GetContentType(string fileName)
        {
            // Create a new FileExtensionContentTypeProvider
            var provider = new FileExtensionContentTypeProvider();

            // Try to get the content type based on the file name's extension
            if (provider.TryGetContentType(fileName, out var contentType))
            {
                return contentType;
            }

            // If the content type cannot be determined, provide a default value
            return "application/octet-stream"; // This is a common default for unknown file types
        }
        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFileAsync(int fileId)
        {
            try { 

            var egressFile = _DbContext.EgressFiles.First(x => x.Id == fileId);
           

            
                var response = await _minioHelper.GetCopyObject(egressFile.EgressSubmission.OutputBucket, egressFile.Name);

                using (var responseStream = response.ResponseStream)
                {
                    var fileBytes = new byte[responseStream.Length];
                    await responseStream.ReadAsync(fileBytes, 0, (int)responseStream.Length);

                    // Create a FileContentResult and return it as the response
                    return File(fileBytes, GetContentType(egressFile.Name), egressFile.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "DownloadFiles");
                throw;
            }

        }


        [Authorize(Roles = "data-egress-admin")]
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

