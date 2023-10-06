
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

namespace TRE_API.Controllers
{
    [Authorize(Roles = "dare-tre-agent")]
    //[AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : Controller
    {
        private readonly ISignalRService _signalRService;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly IDataEgressClientHelper  _dataEgressHelper;
        private readonly IHutchClientHelper _hutchHelper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IBus _rabbit;
        private readonly ISubmissionHelper _subHelper;
        private readonly IMinioHelper _minioHelper;
        private readonly MinioSettings _minioSettings;

        public SubmissionController(ISignalRService signalRService, IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext, IBus rabbit, ISubmissionHelper subHelper, IDataEgressClientHelper egressHelper, IHutchClientHelper hutchClientHelper, IMinioHelper minioHelper, MinioSettings minioSettings)
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


        [HttpPost("DAREUpdateSubmission")]
        public async void DAREUpdateSubmission(string trename, string tesId, string submissionStatus)
        {
            List<string> StringList = new List<string> { trename, tesId, submissionStatus };
            await _signalRService.SendUpdateMessage("TREUpdateStatus", StringList);
        }



        [HttpGet("IsUserApprovedOnProject")]
        public BoolReturn IsUserApprovedOnProject(int projectId, int userId)
        {

            return new BoolReturn()
            {
                Result = _subHelper.IsUserApprovedOnProject(projectId,userId)
            };
        }

        [HttpGet]
        [Route("GetWaitingSubmissionsForTre")]
        [ValidateModelState]
        [SwaggerOperation("GetWaitingSubmissionsForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetWaitingSubmissionsForTre()
        {
            var result =_subHelper.GetWaitingSubmissionForTre();


            return StatusCode(200, result);
        }

       


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

        [HttpGet]
        [Route("GetOutputBucketInfo")]
        [ValidateModelState]
        [SwaggerOperation("GetOutputBucketInfo")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public IActionResult GetOutputBucketInfo(string subId)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("submissionId", subId.ToString());
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;

            var bucket = _dbContext.Projects
                .Where(x => x.SubmissionProjectId == submission.Project.Id)
                .Select(x => new { x.OutputBucketTre });

            var outputBucket = bucket.FirstOrDefault();

            var status = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                new Dictionary<string, string>()
                {
                    { "tesId", submission.TesId }, { "statusType", StatusType.PodProcessingComplete.ToString() },
                    { "description", "" }
                }).Result;

            return StatusCode(200, outputBucket);
        }

        [HttpPost]
        [Route("FilesReadyForReview")]
        [ValidateModelState]
        [SwaggerOperation("FilesReadyForReview")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public IActionResult FilesReadyForReview([FromBody] ReviewFiles review)
        {
            var submission = _dataEgressHelper.CallAPI<ReviewFiles, Submission>("/api/DataEgress/AddNewDataEgress/", review).Result;
            return StatusCode(200, submission);
        }

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
            var HUTCHres = _hutchHelper.CallAPI<EgressReview, APIReturn>("HUTCH URL", review);

            //Send the submission, this is the list of files that have been approved and rejected
            //var fileListString = JsonConvert.SerializeObject(EgressFileList);

            //var HUTCHParams = new Dictionary<string, string>()
            //{
            //    { "submissionId", submissionId.ToString() },
            //    { "fileList", fileListString}
            //};

            //var HUTCHres = _hutchHelper.CallAPIWithoutModel<APIReturn>("URl for hutch", HUTCHParams);

            return StatusCode(200);
        }

        [HttpPost]
        [Route("FinalOutcome")]
        [ValidateModelState]
        [SwaggerOperation("FinalOutcome")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        public IActionResult FinalOutcome([FromBody] FinalOutcome outcome)
        {
            //For me to code
            var statusParams = new Dictionary<string, string>()
                                    {
                                        { "tesId", outcome.subId.ToString() },
                                        { "statusType", StatusType.Completed.ToString() },
                                        { "description", "" }
                                    };

            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

            //Copy file to output bucket
            //var copyResult = _minioHelper.CopyObject(_minioSettings, sourceBucket, destinationBucket, fileName, fileName);

            throw new NotImplementedException();
        }

        //[HttpGet]
        //[Route("GetBucketInfo")]
        //[ValidateModelState]
        //[SwaggerOperation("GetBucketInfo")]
        //[SwaggerResponse(statusCode: 200, type: typeof(string), description: "")]
        //public IActionResult GetBucketInfo(string submissionId)
        //{
        //    var paramlist = new Dictionary<string, string>();
        //    paramlist.Add("submissionId", submissionId.ToString());
        //    var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
        //        .Result;

        //    var bucket = _dbContext.Projects
        //        .Where(x => x.SubmissionProjectId == submission.Project.Id)
        //        .Select(x => new { x.OutputBucketTre });

        //    var outputBucket = bucket.FirstOrDefault();

        //    var status = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
        //        new Dictionary<string, string>()
        //        {
        //            { "tesId", submission.TesId }, { "statusType", StatusType.PodProcessingComplete.ToString() },
        //            { "description", "" }
        //        }).Result;

        //    return StatusCode(200, outputBucket);
        //}

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

        //[HttpPost]
        //[Route("SaveHutchFiles")]
        //[ValidateModelState]
        //[SwaggerOperation("SaveHutchFiles")]
        //[SwaggerResponse(statusCode: 200, type: typeof(Submission), description: "")]
        //public IActionResult SaveHutchFiles(string submissionId, List<SubmissionFile> submissionFiles)
        //{
        //    var paramlist = new Dictionary<string, string>();
        //    paramlist.Add("submissionId", submissionId.ToString());
        //    var submission = _dataEgressHelper.CallAPI<List<SubmissionFile>, Submission>("/api/DataEgress/AddNewDataEgress/", submissionFiles,
        //            paramlist).Result; 
        //    return StatusCode(200, submission);
        //}


        //[HttpPost("SendFileResultsToHUTCH")]
        //public IActionResult SendFileResultsToHUTCH(int submissionId, List<DataFiles> EgressFileList)
        //{
        //    //Update status of submission to "Sending to hutch for final packaging"
        //    var statusParams = new Dictionary<string, string>()
        //                            {
        //                                { "tesId", submissionId.ToString() },
        //                                { "statusType", StatusType.SendingToHUTCHForFinalPackaging.ToString() },
        //                                { "description", "" }
        //                            };
        //    var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

        //    //Send the submission, this is the list of files that have been approved and rejected
        //    var fileListString = JsonConvert.SerializeObject(EgressFileList);

        //    var HUTCHParams = new Dictionary<string, string>() 
        //    {
        //        { "submissionId", submissionId.ToString() },
        //        { "fileList", fileListString}
        //    };

        //    var HUTCHres = _hutchHelper.CallAPIWithoutModel<APIReturn>("URl for hutch", HUTCHParams);

        //    return StatusCode(200);
        //}

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
