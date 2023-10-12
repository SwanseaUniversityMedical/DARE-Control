
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
        private readonly IMinioHelper _minioHelper;
        private readonly MinioSettings _minioSettings;

        public SubmissionController(ISignalRService signalRService, IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext, IBus rabbit, ISubmissionHelper subHelper, IDataEgressClientWithoutTokenHelper egressHelper, IHutchClientHelper hutchClientHelper, IMinioHelper minioHelper, MinioSettings minioSettings)
        {
            _signalRService = signalRService;
            _dareHelper = helper;
            _dataEgressHelper = egressHelper;
            _hutchHelper = hutchClientHelper;
            _dbContext = dbContext;
            _rabbit = rabbit;
            _subHelper = subHelper;
            _minioHelper = minioHelper;
            _minioSettings = minioSettings;
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpPost("DAREUpdateSubmission")]
        public async void DAREUpdateSubmission(string trename, string tesId, string submissionStatus)
        {
            List<string> StringList = new List<string> { trename, tesId, submissionStatus };
            await _signalRService.SendUpdateMessage("TREUpdateStatus", StringList);
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


        [Authorize(Roles = "dare-tre-admin")]
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
            var outputFolder = GetOutputBucketGuts(subId);

            var status = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                new Dictionary<string, string>()
                {
                    { "tesId", outputFolder.TesId }, { "statusType", StatusType.PodProcessingComplete.ToString() },
                    { "description", "" }
                }).Result;

            return StatusCode(200, outputFolder.OutputBucket);
        }

        private class OutputBucketInfo
        {
            public string TesId { get; set; }
            public string OutputBucket { get; set; }
        }

        private OutputBucketInfo GetOutputBucketGuts(string subId)
        {
            var outputFolder = "";
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("submissionId", subId.ToString());
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;

            var bucket = _dbContext.Projects
                .Where(x => x.SubmissionProjectId == submission.Project.Id)
                .Select(x => x.OutputBucketTre);

            var outputBucket = bucket.FirstOrDefault();

            var isFolderExists = _minioHelper.FolderExists(_minioSettings, outputBucket.ToString(), "sub" + subId).Result;
            if (!isFolderExists)
            {
                var submissionFolder = _minioHelper.CreateFolder(_minioSettings, outputBucket.ToString(), "sub" + subId).Result;
            }

            outputFolder = outputBucket.ToString() + "/" + "sub" + subId;
            return new OutputBucketInfo()
            {
                OutputBucket = outputFolder,
                TesId = submission.TesId
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

            var egsub = new EgressSubmission()
            {
                SubmissionId = review.subId,
                OutputBucket = GetOutputBucketGuts(review.subId).OutputBucket,
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
        
        [Authorize(Roles = "dare-tre-admin,data-egress-admin")]
        [HttpPost]
        [Route("EgressResults")]
        [ValidateModelState]
        [SwaggerOperation("EgressResults")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public IActionResult EgressResults([FromBody] EgressReview review)
        {
            //Update status of submission to "Sending to hutch for final packaging"
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "tesId", review.subId.ToString() },
                                        { "statusType", StatusType.SendingToHUTCHForFinalPackaging.ToString() },
                                        { "description", "" }
                                    };
            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

            //Not sure what the return type is
            var HUTCHres = _hutchHelper.CallAPI<EgressReview, APIReturn>("HUTCH URL", review);

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
            paramlist.Add("submissionId", outcome.subId.ToString());
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;

            var bucket = _dbContext.Projects
                .Where(x => x.SubmissionProjectId == submission.Project.Id)
                .Select(x => new { x.OutputBucketTre });

            var sourceBucket = bucket.FirstOrDefault().OutputBucketTre;

            var paramlist2 = new Dictionary<string, string>();
            paramlist2.Add("projectId", submission.Project.Id.ToString());
            var project = _dareHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist2).Result;

            var destinationBucket = project.OutputBucket;

            //For me to code
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "tesId", outcome.subId.ToString() },
                                        { "statusType", StatusType.Completed.ToString() },
                                        { "description", "" }
                                    };

            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

            //Copy file to output bucket
            var copyResult = _minioHelper.CopyObject(_minioSettings, sourceBucket, destinationBucket, "sub" + outcome.subId+"/" + outcome.file, "sub" + outcome.subId + "/" + outcome.file);
            var boolresult = new BoolReturn()
            {
                Result = copyResult.Result
            };
            return StatusCode(200, copyResult);
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet]
        [Route("DataOutApproval")]
        [ValidateModelState]
        [SwaggerOperation("DataOutApproval")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult DataOutApproval(string submissionId, bool isApproved)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("submissionId", submissionId.ToString());
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;

            var status = "";
            if (isApproved)
            {
                status = StatusType.DataOutApproved.ToString();
            }
            else
            {
                status = StatusType.DataOutApprovalRejected.ToString();
            }

            var result = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                new Dictionary<string, string>()
                    { { "tesId", submission.TesId }, { "statusType", status }, { "description", "" } }).Result;

            return StatusCode(200, result);
        }

        [AllowAnonymous]
        [HttpPost("TestFetchAndStore")]
        public void TestFetchAndStore([FromBody] MQFetchFile message)
        {

            var exch = _rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Main, "topic");

            _rabbit.Advanced.Publish(exch, RoutingConstants.FetchFile, false, new Message<MQFetchFile>(message));
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpPost("SendSubmissionToHUTCH")]
        public IActionResult SendSubmissionToHUTCH(Dictionary<string, string> SubmissionData)
        {
            //Update status of submission to "Sending to hutch"
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "tesId", SubmissionData["SubmissionId"] },
                                        { "statusType", StatusType.SendingFileToHUTCH.ToString() },
                                        { "description", "" }
                                    };
            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

            var res = _hutchHelper.CallAPIWithoutModel<APIReturn>("URL for hutch", SubmissionData); //Need to update this when parameters are known

            return StatusCode(200);
        }

    }
}
