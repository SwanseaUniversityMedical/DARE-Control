using BL.Models;
using BL.Models.Enums;
using DARE_API.Repositories.DbContexts;
using DARE_API.Attributes;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using BL.Models.ViewModels;
using DARE_API.Services;
using EasyNetQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BL.Rabbit;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Xml.Linq;
using BL.Services;
using Microsoft.AspNetCore.StaticFiles;

namespace DARE_API.Controllers
{

    [Route("api/[controller]")]
    
    [ApiController]
    

    /// <summary>
    /// API endpoints for <see cref="Submission"/>s.
    /// </summary>
    public class SubmissionController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        private readonly IBus _rabbit;
        private readonly IMinioHelper _minioHelper;


        public SubmissionController(ApplicationDbContext repository, IBus rabbit, IMinioHelper minioHelper)
        {
            _DbContext = repository;
            _rabbit = rabbit;
            _minioHelper = minioHelper;


        }

        
        [Authorize(Roles = "dare-control-admin,dare-tre-admin")]
        [HttpGet]
        [Route("GetWaitingSubmissionsForTre")]
        [ValidateModelState]
        [SwaggerOperation("GetWaitingSubmissionsForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetWaitingSubmissionsForTre()
        {

            var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            var tre = _DbContext.Tres.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName);
            if (tre == null)
            {
                return BadRequest("User " + usersName + " doesn't have a tre");
            }

            tre.LastHeartBeatReceived = DateTime.Now.ToUniversalTime();
            _DbContext.SaveChanges();
            var results = tre.Submissions.Where(x => x.Status == StatusType.WaitingForAgentToTransfer).ToList();


            return StatusCode(200, results);
        }

        [Authorize(Roles = "dare-control-admin,dare-tre-admin")]
        [HttpGet]
        [Route("GetRequestCancelSubsForTre")]
        [ValidateModelState]
        [SwaggerOperation("GetRequestCancelSubsForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetRequestCancelSubsForTre()
        {

            var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            var tre = _DbContext.Tres.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName);
            if (tre == null)
            {
                return BadRequest("User " + usersName + " doesn't have a tre");
            }

            tre.LastHeartBeatReceived = DateTime.Now.ToUniversalTime();
            _DbContext.SaveChanges();
            var results = tre.Submissions.Where(x => x.Status == StatusType.RequestCancellation).ToList();


            return StatusCode(200, results);
        }

        [Authorize(Roles = "dare-control-admin,dare-tre-admin")]
        [HttpGet]
        [Route("UpdateStatusForTre")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult UpdateStatusForTre(string subId, StatusType statusType, string? description)
        {

            var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            var tre = _DbContext.Tres.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName.ToLower());
            if (tre == null)
            {
                return BadRequest("User " + usersName + " doesn't have an tre");
            }


            var sub = _DbContext.Submissions.FirstOrDefault(x => x.Id == int.Parse(subId) && x.Tre == tre);
            if (sub == null)
            {
                return BadRequest("Invalid subid or tre not valid for tes");
            }
            if (SubCompleteTypes.Contains(sub.Status))
            {
                throw new Exception("Submission already closed. Can't change status");
            }
            UpdateSubmissionStatus.UpdateStatus(sub, statusType, description);
            _DbContext.SaveChanges();
           


            return StatusCode(200, new APIReturn() { ReturnType = ReturnType.voidReturn });
        }

        [Authorize(Roles = "dare-control-admin,dare-tre-admin")]
        [HttpGet]
        [Route("CloseSubmissionForTre")]
        [ValidateModelState]
        [SwaggerOperation("CloseSubmissionForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult CloseSubmissionForTre(string subId, StatusType statusType, string? finalFile, string? description)
        {
            if (!SubCompleteTypes.Contains(statusType))
            {
                throw new Exception("Invalid completion type");
            }
            var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            var tre = _DbContext.Tres.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName.ToLower());
            if (tre == null)
            {
                return BadRequest("User " + usersName + " doesn't have an tre");
            }


            var sub = _DbContext.Submissions.FirstOrDefault(x => x.Id == int.Parse(subId) && x.Tre == tre);
            if (sub == null)
            {
                return BadRequest("Invalid subid or tre not valid for tes");
            }
            if (SubCompleteTypes.Contains(sub.Status))
            {
                throw new Exception("Submission already closed. Can't change status");
            }

            UpdateSubmissionStatus.UpdateStatus(sub, statusType, description);
            sub.FinalOutputFile = finalFile;
            _DbContext.SaveChanges();
            var parentStatus = StatusType.WaitingForChildSubsToComplete;
            if (sub.Parent.Children.All(x => SubCompleteTypes.Contains(x.Status)))
            {
                if (sub.Parent.Children.All(x => x.Status == StatusType.Failed))
                {
                    UpdateSubmissionStatus.UpdateStatus(sub.Parent, StatusType.Failed, "");

                }
                else if (sub.Parent.Children.All(x => x.Status == StatusType.Completed))
                {
                    UpdateSubmissionStatus.UpdateStatus(sub.Parent, StatusType.Completed, "");

                }
                else if (sub.Parent.Children.All(x => x.Status == StatusType.Cancelled))
                {
                    UpdateSubmissionStatus.UpdateStatus(sub.Parent, StatusType.Cancelled, "");
                }
                else
                {
                    UpdateSubmissionStatus.UpdateStatus(sub.Parent, StatusType.PartialResult, "");
                }
                sub.Parent.EndTime = DateTime.Now.ToUniversalTime();
            }

            _DbContext.SaveChanges();
            return StatusCode(200, new APIReturn() { ReturnType = ReturnType.voidReturn });
        }

        [AllowAnonymous]
        [HttpGet("GetAllSubmissions")]
        public List<Submission> GetAllSubmissions()
        {
            try
            {
                var allSubmissions = _DbContext.Submissions.ToList();

                Log.Information("{Function} Submissions retrieved successfully", "GetAllSubmissions");
                return allSubmissions;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllSubmissions");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("TestSubRabbitSendRemoveBeforeDeploy")]
        public void TestSubRabbitSendRemoveBeforeDeploy(int id)
        {
            try { 
            var exch = _rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Main, "topic");

            _rabbit.Advanced.Publish(exch, RoutingConstants.Subs, false, new Message<int>(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "TestSubRabbitSendBeforeDeploy");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("GetASubmission")]
        public Submission GetASubmission(int submissionId)
        {
            try
            {

                var submission = _DbContext.Submissions.First(x => x.Id == submissionId);

                Log.Information("{Function} Submission retrieved successfully", "GetASubmission");
                return submission;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetASubmission");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("StageTypes")]
        public Stages StageTypes()
        {
            var stage1List = new StageInfo();
            stage1List.stageName = "Submission Layer Processing";
            stage1List.stageNumber = 1;

            stage1List.statusTypeList = new List<StatusType>
            {
                StatusType.WaitingForChildSubsToComplete,
                StatusType.WaitingForAgentToTransfer,
                StatusType.UserNotOnProject,
                StatusType.SubmissionWaitingForCrateFormatCheck,
                StatusType.Running,
                StatusType.SubmissionReceived,
                StatusType.SubmissionCrateValidated,
                StatusType.SubmissionCrateValidationFailed,


            };
            Dictionary<int, List<StatusType>> stage1Dict = new Dictionary<int, List<StatusType>>();
            stage1Dict.Add(1, stage1List.statusTypeList);
            stage1List.stagesDict = stage1Dict;

            var stage2List = new StageInfo();
            stage2List.stageName = "Tre Layer Processing";
            stage2List.stageNumber = 2;
            stage2List.statusTypeList = new List<StatusType>
            {
                StatusType.TransferredToPod,
                StatusType.InvalidUser,
                StatusType.TRENotAuthorisedForProject,
                StatusType.AgentTransferringToPod,
                StatusType.TransferToPodFailed,
                StatusType.SendingSubmissionToHutch,
                StatusType.TreCrateValidated,
                StatusType.TreCrateValidationFailed,
                StatusType.TreCrateValidated,


            };
            Dictionary<int, List<StatusType>> stage2Dict = new Dictionary<int, List<StatusType>>();
            stage2Dict.Add(2, stage2List.statusTypeList);
            stage2List.stagesDict = stage2Dict;

            var stage3List = new StageInfo();
            stage3List.stageName = "Query Processing";
            stage3List.stageNumber = 3;
            stage3List.statusTypeList = new List<StatusType>
            {
                StatusType.PodProcessing,
                StatusType.PodProcessingComplete,
                StatusType.PodProcessingFailed,
                StatusType.WaitingForCrate,
                StatusType.FetchingCrate,
                StatusType.Queued,
                StatusType.ValidatingCrate,
                StatusType.FetchingWorkflow,
                StatusType.StagingWorkflow,
                StatusType.ExecutingWorkflow,
                StatusType.PreparingOutputs,
                StatusType.DataOutRequested,
                StatusType.TransferredForDataOut,
                StatusType.PackagingApprovedResults,
                StatusType.Complete,
                StatusType.Failure,





            };
            Dictionary<int, List<StatusType>> stage3Dict = new Dictionary<int, List<StatusType>>();
            stage3Dict.Add(3, stage3List.statusTypeList);
            stage3List.stagesDict = stage3Dict;

            var stage4List = new StageInfo();
            stage4List.stageName = "Data Egress Processing";
            stage4List.stageNumber = 4;
            stage4List.statusTypeList = new List<StatusType>
            {
                StatusType.DataOutApprovalBegun,
                StatusType.DataOutApprovalRejected,
                StatusType.DataOutApproved,
                StatusType.RequestingHutchDoesFinalPackaging
            };
            Dictionary<int, List<StatusType>> stage4Dict = new Dictionary<int, List<StatusType>>();
            stage4Dict.Add(4, stage4List.statusTypeList);
            stage4List.stagesDict = stage4Dict;

            var stage5List = new StageInfo();
            stage5List.stageName = "Final Processing";
            stage5List.stageNumber = 5;

            stage5List.statusTypeList = SubCompleteTypes;
            
            Dictionary<int, List<StatusType>> stage5Dict = new Dictionary<int, List<StatusType>>();
            stage5Dict.Add(5, stage5List.statusTypeList);
            stage5List.stagesDict = stage5Dict;

            List<StageInfo> infoList = new List<StageInfo>();
            // var infoList = new List<List<StageInfo>>();
            infoList.Add(stage1List);
            infoList.Add(stage2List);
            infoList.Add(stage3List);
            infoList.Add(stage4List);
            infoList.Add(stage5List);

            var result = new Stages()
            {
                //GreenStages = new List<StatusType>()
                //{
                //    StatusType.ValidationSuccessful,
                //    StatusType.TransferredToPod,
                //    StatusType.DataOutApproved,
                //    StatusType.PodProcessingComplete,
                //    StatusType.SubmissionCrateValidated,
                //    StatusType.TreCrateValidated,
                //    StatusType.Completed,
                //    StatusType.PartialResult
                //},
                RedStages = new List<StatusType>()
                {
                    StatusType.InvalidUser,
                    StatusType.CancellingChildren,
                    StatusType.UserNotOnProject,
                    StatusType.RequestCancellation,
                    StatusType.CancellationRequestSent,
                    StatusType.Cancelled,
                    StatusType.InvalidSubmission,
                    StatusType.TRENotAuthorisedForProject,
                    StatusType.DataOutApprovalRejected,
                    StatusType.SubmissionCrateValidationFailed,
                    StatusType.TreCrateValidationFailed,
                    StatusType.TransferToPodFailed,
                    StatusType.PodProcessingFailed,
                    StatusType.Failed,
                    
                },
                StageInfos = infoList.OrderBy(x => x.stageNumber).ToList()
            };

            return result;

        }

        //[AllowAnonymous]
        //[HttpGet("DifferentStages")]
        //public Dictionary<int, StageInfo> DifferentStages()
        //{

        //    var stage1List = new StageInfo();
        //    stage1List.stageName = "Submission Layer Validation";
        //    stage1List.stageNumber = 1;
        //    stage1List.statusTypeList = new List<StatusType>
        //    {
        //        StatusType.InvalidUser,
        //        StatusType.UserNotOnProject,
        //        StatusType.InvalidSubmission,
        //        StatusType.SubmissionWaitingForCrateFormatCheck
        //    };
        //    return null;

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
        
        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFileAsync(int submissionId)
        {
            try
            {

                var submission = _DbContext.Submissions.First(x => x.Id == submissionId);



                var response = await _minioHelper.GetCopyObject(submission.Project.OutputBucket, submission.FinalOutputFile);

                using (var responseStream = response.ResponseStream)
                {
                    var fileBytes = new byte[responseStream.Length];
                    await responseStream.ReadAsync(fileBytes, 0, (int)responseStream.Length);

                    // Create a FileContentResult and return it as the response
                    return File(fileBytes, GetContentType(submission.FinalOutputFile), submission.FinalOutputFile);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "DownloadFiles");
                throw;
            }

        }
        public static List<StatusType> SubCompleteTypes =>
            new()
            {

                StatusType.Completed,
                StatusType.Cancelled,
                StatusType.Failed,
                StatusType.PartialResult
            };

        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("SaveSubmissionFiles")]
        public IActionResult SaveSubmissionFiles(int submissionId, List<SubmissionFile> submissionFiles)
        {
            try { 
            var existingSubmission = _DbContext.Submissions
                .Include(d => d.SubmissionFiles)
                .FirstOrDefault(d => d.Id == submissionId);

            if (existingSubmission != null)
            {
                foreach (var file in submissionFiles)
                {
                    var existingFile =
                        existingSubmission.SubmissionFiles.FirstOrDefault(f =>
                            f.TreBucketFullPath == file.TreBucketFullPath);
                    if (existingFile != null)
                    {
                        existingFile.Name = file.Name;
                        existingFile.TreBucketFullPath = file.TreBucketFullPath;
                        existingFile.SubmisionBucketFullPath = file.SubmisionBucketFullPath;
                        existingFile.Status = file.Status;
                        existingFile.Description = file.Description;
                    }
                    else
                    {
                        existingSubmission.SubmissionFiles.Add(file);
                    }
                }

                _DbContext.SaveChanges();
                return Ok(existingSubmission);
            }
            else
            {
                return BadRequest();
            }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "SaveSubmissionFiles");
                throw;
            }
        }
    }
}
