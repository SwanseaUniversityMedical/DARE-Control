
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


        public SubmissionController(ISignalRService signalRService, IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext, IBus rabbit, ISubmissionHelper subHelper, IDataEgressClientWithoutTokenHelper egressHelper, IHutchClientHelper hutchClientHelper, IMinioSubHelper minioSubHelper, IMinioTreHelper minioTreHelper)
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
            var result = _subHelper.GetWaitingSubmissionForTre();
            return StatusCode(200, result);
        }


        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpPost]
        [Route("UpdateStatusForTre")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult UpdateStatusForTre([FromBody] SubmissionDetails subDetails)
        {
            APIReturn? result = _subHelper.UpdateStatusForTre(subDetails.subId, subDetails.statusType, subDetails.description);
            return StatusCode(200, result);
        }

        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpGet]
        [Route("GetOutputBucketInfo")]
        [ValidateModelState]
        [SwaggerOperation("GetOutputBucketInfo")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public IActionResult GetOutputBucketInfo(string subId)
        {
            var outputInfo = GetOutputBucketGuts(subId);

            var status = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                new Dictionary<string, string>()
                {
                    { "subId", outputInfo.SubId }, { "statusType", StatusType.PodProcessingComplete.ToString() },
                    { "description", "" }
                }).Result;

            return StatusCode(200, outputInfo);
        }

        public class OutputBucketInfo
        {
            public string SubId { get; set; }
            public string OutputBucket { get; set; }
            public string OutputFolder { get; set; }
        }

        private OutputBucketInfo GetOutputBucketGuts(string subId)
        {
            
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
                OutputBucket = outputBucket,
                SubId = submission.Id.ToString(),
                OutputFolder = "/" + "sub" + subId + "/"
        };
        }

        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpPost]
        [Route("FilesReadyForReview")]
        [ValidateModelState]
        [SwaggerOperation("FilesReadyForReview")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public IActionResult FilesReadyForReview([FromBody] ReviewFiles review)
        {

            var bucket = GetOutputBucketGuts(review.subId);
            var egsub = new EgressSubmission()
            {
                SubmissionId = review.subId,
                OutputBucket = bucket.OutputBucket,
                SubFolder = bucket.OutputFolder,
                Status = EgressStatus.NotCompleted, 
                Files = new List<EgressFile>()
            };

            foreach (var reviewFile in review.files)
            {
                egsub.Files.Add(new EgressFile()
                {
                    Name = reviewFile,
                    Status = FileStatus.Undecided
                });
            }
            var boolResult = _dataEgressHelper.CallAPI<EgressSubmission, BoolReturn>("/api/DataEgress/AddNewDataEgress/", egsub).Result;
            return StatusCode(200, boolResult);
        }
        
       

        [HttpPost]
        [Route("EgressResults")]
        [ValidateModelState]
        [SwaggerOperation("EgressResults")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public async Task<IActionResult> EgressResults([FromBody] EgressReview review)
        {
            //Update status of submission to "Sending to hutch for final packaging"
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "subId", review.subId.ToString() },
                                        { "statusType", StatusType.SendingToHUTCHForFinalPackaging.ToString() },
                                        { "description", "" }
                                    };
            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

            Dictionary<string, bool> hutchRes = new Dictionary<string, bool>();
            ApprovalType approvalStatus;
            var approvedCount = review.fileResults.Count(x => x.approved);
            var rejectedCount = review.fileResults.Count(x => !x.approved);

            if (approvedCount == review.fileResults.Count)
            {
                approvalStatus = ApprovalType.FullyApproved;
            }
            else if (rejectedCount == review.fileResults.Count)
            {
                approvalStatus = ApprovalType.NotApproved;
            }
            else
            {
                approvalStatus = ApprovalType.PartiallyApproved;
            }
            foreach (var i in review.fileResults)
            {
                hutchRes.Add(i.fileName, i.approved);

            }

            var bucket = GetOutputBucketGuts(review.subId);
            ApprovalResult hutchPayload = new ApprovalResult()
            {
                OutputBucket = bucket.OutputBucket,
                SubFolder = bucket.OutputFolder,
                Status = approvalStatus,
                FileResults = hutchRes
            };

            //Not sure what the return type is
            var HUTCHres = await _hutchHelper.CallAPI<ApprovalResult, APIReturn>($"/api/jobs/{review.subId}/approval", hutchPayload);

            return StatusCode(200, HUTCHres);
        }

        


        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpPost]
        [Route("FinalOutcome")]
        [ValidateModelState]
        [SwaggerOperation("FinalOutcome")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public IActionResult FinalOutcome([FromBody] FinalOutcome outcome)
        {

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("submissionId", outcome.subId);
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;
            var sourceBucket = GetOutputBucketGuts(outcome.subId);
            

            var paramlist2 = new Dictionary<string, string>();
            paramlist2.Add("projectId", submission.Project.Id.ToString());
            var project = _dareHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist2).Result;

            var destinationBucket = project.OutputBucket;

            //Copy file to output bucket
            var source = _minioTreHelper.GetCopyObject(sourceBucket.OutputBucket, sourceBucket.OutputFolder + outcome.file);
            var copyResult = _minioSubHelper.CopyObjectToDestination(destinationBucket, sourceBucket.OutputFolder + outcome.file, source.Result);
            //For me to code
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "subId", outcome.subId.ToString() },
                                        { "statusType", StatusType.Completed.ToString() },
                                        { "description", "" }
                                    };

            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

           
            
            var boolresult = new BoolReturn()
            {
                Result = copyResult.Result
            };
            return StatusCode(200, copyResult);
        }



        [HttpPost("SendSubmissionToHUTCH")]
        public IActionResult SendSubmissionToHUTCH(Submission sub)
        {
            //Update status of submission to "Sending to hutch"
            _subHelper.SendSumissionToHUTCH(sub);

            return StatusCode(200);
        }

        
    }
}
