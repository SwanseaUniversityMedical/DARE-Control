
using BL.Models.ViewModels;
using BL.Models;
using BL.Models.Enums;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TRE_API.Attributes;
using TRE_API.Services;
using TRE_API.Services.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using BL.Models.APISimpleTypeReturns;
using TRE_API.Repositories.DbContexts;
using EasyNetQ.Management.Client.Model;
using BL.Rabbit;
using EasyNetQ;
using Newtonsoft.Json;
using System;
using Amazon.Runtime.Internal.Transform;
using Serilog;

namespace TRE_API.Controllers
{
    
    
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : Controller
    {
        private readonly ISignalRService _signalRService;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly IDataEgressClientWithoutTokenHelper  _dataEgressHelper;
        private readonly IHutchClientHelper _hutchHelper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IBus _rabbit;
        private readonly ISubmissionHelper _subHelper;
        private readonly IMinioSubHelper _minioSubHelper;
        private readonly IMinioTreHelper _minioTreHelper;
        private readonly MinioTRESettings _minioTreSettings;


        public SubmissionController(ISignalRService signalRService, IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext, IBus rabbit, ISubmissionHelper subHelper, IDataEgressClientWithoutTokenHelper egressHelper, IHutchClientHelper hutchClientHelper, IMinioSubHelper minioSubHelper, IMinioTreHelper minioTreHelper, MinioTRESettings minioTreSettings)
        {
            _signalRService = signalRService;
            _dareHelper = helper;
            _dataEgressHelper = egressHelper;
            _hutchHelper = hutchClientHelper;
            _dbContext = dbContext;
            _rabbit = rabbit;
            _subHelper = subHelper;
            _minioTreHelper = minioTreHelper;
            _minioSubHelper = minioSubHelper;
            _minioTreSettings = minioTreSettings;

        }

       


        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("IsUserApprovedOnProject")]
        public BoolReturn IsUserApprovedOnProject(int projectId, int userId)
        {

            return new BoolReturn()
            {
                Result = _subHelper.IsUserApprovedOnProject(projectId, userId)
            };
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet]
        [Route("GetWaitingSubmissionsForTre")]
        [ValidateModelState]
        [SwaggerOperation("GetWaitingSubmissionsForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetWaitingSubmissionsForTre()
        {
            try { 
            var result = _subHelper.GetWaitingSubmissionForTre();
            return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "GetWaitingSubmissionsForTre");
                throw;
            }
        }


        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpPost]
        [Route("UpdateStatusForTre")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult UpdateStatusForTre([FromBody] SubmissionDetails subDetails)
        {
            try { 
            APIReturn? result = _subHelper.UpdateStatusForTre(subDetails.SubId, subDetails.StatusType, subDetails.Description);
            return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "UpdateStatusForTre");
                throw;
            }
        }
        
        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpGet]
        [Route("GetOutputBucketInfo")]
        [ValidateModelState]
        [SwaggerOperation("GetOutputBucketInfo")]
        [SwaggerResponse(statusCode: 200, type: typeof(OutputBucketInfo), description: "")]
        public IActionResult GetOutputBucketInfo(string subId)
        {
            try { 
            var outputInfo = _subHelper.GetOutputBucketGuts(subId);


                var status = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                    new Dictionary<string, string>()
                    {
                    { "subId", outputInfo.SubId }, { "statusType", StatusType.PodProcessingComplete.ToString() },
                    { "description", "" }
                    }).Result;

                return StatusCode(200, outputInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "GetOutputBucketInfo");
                throw;
            }
        }

        public class OutputBucketInfo
        {
            public string Host { get; set; }
            public string SubId { get; set; }
            public string Bucket { get; set; }
            public string Path { get; set; }
        }

        private OutputBucketInfo GetOutputBucketGuts(string subId)
        {
            try { 
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("submissionId", subId.ToString());
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;

            var bucket = _dbContext.Projects
                .Where(x => x.SubmissionProjectId == submission.Project.Id)
                .Select(x => x.OutputBucketTre);

            var outputBucket = bucket.FirstOrDefault();

            var isFolderExists = _minioTreHelper.FolderExists(outputBucket.ToString(), "sub" + subId).Result;
            if (!isFolderExists)
            {
                var submissionFolder = _minioTreHelper.CreateFolder(outputBucket.ToString(), "sub" + subId).Result;
            }

            outputBucket = outputBucket.ToString();
            return new OutputBucketInfo()
            {
                Bucket = outputBucket,
                SubId = submission.Id.ToString(),
                Path = "sub" + subId + "/",
                Host = _minioTreSettings.Url
            };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "GetOutputBucketGuts");
                throw;
            }
        }

        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpPost]
        [Route("FilesReadyForReview")]
        [ValidateModelState]
        [SwaggerOperation("FilesReadyForReview")]
        [SwaggerResponse(statusCode: 200, type: typeof(BoolReturn), description: "")]
        public IActionResult FilesReadyForReview([FromBody] ReviewFiles review)
        {
            var boolResult = _subHelper.FilesReadyForReview(review);

            return StatusCode(200, boolResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "FilesReadyForReview");
                throw;
            }
        }
        
       

        [HttpPost]
        [Route("EgressResults")]
        [ValidateModelState]
        [SwaggerOperation("EgressResults")]
        [SwaggerResponse(statusCode: 200, type: typeof(BoolReturn), description: "")]
        public async Task<IActionResult> EgressResults([FromBody] EgressReview review)
        {
            try { 
            //Update status of submission to "Sending to hutch for final packaging"
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "subId", review.SubId.ToString() },
                                        { "statusType", StatusType.SendingToHUTCHForFinalPackaging.ToString() },
                                        { "description", "" }
                                    };
            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

            Dictionary<string, bool> hutchRes = new Dictionary<string, bool>();
            ApprovalType approvalStatus;
            var approvedCount = review.FileResults.Count(x => x.Approved);
            var rejectedCount = review.FileResults.Count(x => !x.Approved);

            if (approvedCount == review.FileResults.Count)
            {
                approvalStatus = ApprovalType.FullyApproved;
            }
            else if (rejectedCount == review.FileResults.Count)
            {
                approvalStatus = ApprovalType.NotApproved;
            }
            else
            {
                approvalStatus = ApprovalType.PartiallyApproved;
            }
            foreach (var i in review.FileResults)
            {
                hutchRes.Add(i.FileName, i.Approved);

            }

            var bucket = _subHelper.GetOutputBucketGuts(review.subId);

            ApprovalResult hutchPayload = new ApprovalResult()
            {
                Host = _minioTreSettings.Url,
                Bucket = bucket.Bucket,
                Path = bucket.Path,
                Status = approvalStatus,
                FileResults = hutchRes
            };

            //Not sure what the return type is
            var HUTCHres = await _hutchHelper.CallAPI<ApprovalResult, APIReturn>($"/api/jobs/{review.SubId}/approval", hutchPayload);

            return StatusCode(200, new BoolReturn(){Result = true});
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "EgressResults");
                throw;
            }
        }

        


        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpPost]
        [Route("FinalOutcome")]
        [ValidateModelState]
        [SwaggerOperation("FinalOutcome")]
        [SwaggerResponse(statusCode: 200, type: typeof(BoolReturn), description: "")]
        public IActionResult FinalOutcome([FromBody] FinalOutcome outcome)
        {
            try { 
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("submissionId", outcome.SubId);
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;
            var sourceBucket = _subHelper.GetOutputBucketGuts(outcome.subId);

            

            var paramlist2 = new Dictionary<string, string>();
            paramlist2.Add("projectId", submission.Project.Id.ToString());
            var project = _dareHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist2).Result;

            var destinationBucket = project.OutputBucket;

            //Copy file to output bucket
            var source = _minioTreHelper.GetCopyObject(sourceBucket.Bucket,  outcome.File);
            var isFolderExists = _minioTreHelper.FolderExists(destinationBucket, "sub" + submission.Id).Result;
            if (!isFolderExists)
            {
                var submissionFolder = _minioTreHelper.CreateFolder(destinationBucket, "sub" + submission.Id).Result;
            }
            var copyResult = _minioSubHelper.CopyObjectToDestination(destinationBucket, outcome.File, source.Result);
            //For me to code
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "subId", outcome.SubId.ToString() },
                                        { "statusType", StatusType.Completed.ToString() },
                                        { "description", "" }
                                    };

            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

           
            
            var boolresult = new BoolReturn()
            {
                Result = copyResult.Result
            };
            return StatusCode(200, boolresult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "FinalOutcomeSubmission");
                throw;
            }
        }



        [HttpPost("SendSubmissionToHUTCH")]
        public IActionResult SendSubmissionToHUTCH(Submission sub)
        {
            try { 
            //Update status of submission to "Sending to hutch"
            _subHelper.SendSumissionToHUTCH(sub);

            return StatusCode(200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "SendSubmissionToHUTCH");
                throw;
            }
        }

        
    }
}
