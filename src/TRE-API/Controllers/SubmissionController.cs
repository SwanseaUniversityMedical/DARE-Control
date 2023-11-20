
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
using TRE_API.Models;
using Minio;

namespace TRE_API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : Controller
    {
        private readonly ISignalRService _signalRService;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly IDataEgressClientWithoutTokenHelper _dataEgressHelper;
        private readonly IHutchClientHelper _hutchHelper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IBus _rabbit;
        private readonly ISubmissionHelper _subHelper;
        private readonly IMinioSubHelper _minioSubHelper;
        private readonly IMinioTreHelper _minioTreHelper;
        private readonly MinioTRESettings _minioTreSettings;
        private readonly string _treName;
        private readonly AgentSettings _agentSettings;

        public SubmissionController(ISignalRService signalRService, IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext, 
            IBus rabbit, 
            ISubmissionHelper subHelper,
            IDataEgressClientWithoutTokenHelper egressHelper, 
            IHutchClientHelper hutchClientHelper,
            IMinioSubHelper minioSubHelper,
            IMinioTreHelper minioTreHelper,
            MinioTRESettings minioTreSettings, 
            IConfiguration config,
            AgentSettings agentSettings)
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
            _treName = config["TreName"];
            _agentSettings = agentSettings;
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
            try
            {
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
        // public IActionResult UpdateStatusForTre([FromBody] SubmissionDetails subDetails)
        public IActionResult UpdateStatusForTre(
            [FromQuery] string subId,
            [FromQuery] StatusType statusType,
            [FromQuery] string? description)
        {
            var subDetails = new SubmissionDetails()
            {
                StatusType = statusType,
                SubId = subId,
                Description = description
            };
            if (!EnumHelper.GetHutchAllowedStatusUpdates().Contains(subDetails.StatusType))
            {
                throw new Exception("Restricted StatusType");
            }

            try
            {
                APIReturn? result =
                    _subHelper.UpdateStatusForTre(subDetails.SubId, subDetails.StatusType, subDetails.Description);
                if (subDetails.StatusType == StatusType.Failure || subDetails.StatusType == StatusType.Cancelled)
                {
                    _subHelper.CloseSubmissionForTre(subDetails.SubId, subDetails.StatusType, "", "");
                }
                return StatusCode(200, result);
            }
            catch (Exception ex) {
                
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
            try
            {
                var outputInfo = _subHelper.GetOutputBucketGuts(subId, true);
                

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
            public bool Secure { get; set; }
        }

        

        [Authorize(Roles = "dare-hutch-admin,dare-tre-admin")]
        [HttpPost]
        [Route("FilesReadyForReview")]
        [ValidateModelState]
        [SwaggerOperation("FilesReadyForReview")]
        [SwaggerResponse(statusCode: 200, type: typeof(BoolReturn), description: "")]
        public IActionResult FilesReadyForReview([FromBody] ReviewFiles review)
        {
            try
            {
                _subHelper.UpdateStatusForTre(review.SubId, StatusType.DataOutRequested, "");
                var bucket = _subHelper.GetOutputBucketGuts(review.SubId, false);
                var egsub = new EgressSubmission()
                {
                    SubmissionId = review.SubId,
                    OutputBucket = bucket.Bucket,
                    Status = EgressStatus.NotCompleted,
                    Files = new List<EgressFile>()
                };

                foreach (var reviewFile in review.Files)
                {
                    egsub.Files.Add(new EgressFile()
                    {
                        Name = reviewFile,
                        Status = FileStatus.Undecided
                    });
                }

                var boolResult = _dataEgressHelper
                    .CallAPI<EgressSubmission, BoolReturn>("/api/DataEgress/AddNewDataEgress/", egsub).Result;
                _subHelper.UpdateStatusForTre(review.SubId, StatusType.DataOutApprovalBegun, "");

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
            try
            {  
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

                if (approvalStatus != ApprovalType.FullyApproved)
                {
                    _subHelper.UpdateStatusForTre(review.SubId.ToString(), StatusType.DataOutApprovalRejected, "");
                    

                    var StatusResult = _subHelper.CloseSubmissionForTre(review.SubId, StatusType.Failed, "", "");
                    
                }
                else
                {
                    _subHelper.UpdateStatusForTre(review.SubId.ToString(), StatusType.DataOutApproved, "");
                }
                bool secure = !_minioTreSettings.Url.ToLower().StartsWith("http://");
                var bucket = _subHelper.GetOutputBucketGutsSub(review.SubId, true);
                ApprovalResult hutchPayload = new ApprovalResult()
                {
                    Host = _minioTreSettings.Url.Replace("https://", "").Replace("http://", ""),
                    Bucket = bucket.Bucket,
                    Path = bucket.Path,
                    Secure = secure,
                    Status = approvalStatus,
                    FileResults = hutchRes
                };
                //Only send if fully approved
                if (approvalStatus == ApprovalType.FullyApproved)
                {
                    _subHelper.UpdateStatusForTre(review.SubId, StatusType.RequestingHutchDoesFinalPackaging, "");
                }

                if (_agentSettings.UseTESK == false)
                {
                    //Not sure what the return type is
                    var HUTCHres =
                        await _hutchHelper.CallAPI<ApprovalResult, APIReturn>($"/api/jobs/{review.SubId}/approval",
                            hutchPayload);
                }
                else
                {
                    Log.Information($"EgressResults with review.OutputBucket > {review.OutputBucket} bucket.Bucket > {bucket.Bucket} ");
                    foreach (var File in review.FileResults)
                    {
                        Log.Information($"EgressResults with File.Approved > {File.Approved} File.FileName > {File.FileName} ");
                        if (File.Approved)
                        {
                            var source = _minioTreHelper.GetCopyObject(review.OutputBucket,  File.FileName);
                            var resultcopy = _minioSubHelper.CopyObjectToDestination(bucket.Bucket,  File.FileName, source.Result).Result;
                        }
                    }

                    _subHelper.UpdateStatusForTre(review.SubId, StatusType.Completed, "");
                }




                return StatusCode(200, new BoolReturn() { Result = true });
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
            try
            {
                var exch = _rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Tre, "topic");

                _rabbit.Advanced.Publish(exch, RoutingConstants.ProcessFinalOutput, false, new Message<FinalOutcome>(outcome));

                var boolresult = new BoolReturn()
                {
                    Result = true
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
            try
            {
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
