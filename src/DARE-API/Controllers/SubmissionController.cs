using BL.Models;
using BL.Models.APISimpleTypeReturns;
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

namespace DARE_API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Roles = "dare-control-admin,dare-tre-admin")]
    [ApiController]


    /// <summary>
    /// API endpoints for <see cref="Submission"/>s.
    /// </summary>
    public class SubmissionController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        private readonly IBus _rabbit;

        private readonly IWebHostEnvironment _environment;
        public SubmissionController(ApplicationDbContext repository, IBus rabbit, IWebHostEnvironment environment)
        {
            _DbContext = repository;
            _rabbit = rabbit;
            _environment = environment;



        }



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


        [HttpGet]
        [Route("UpdateStatusForTre")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult UpdateStatusForTre(string tesId, StatusType statusType, string? description)
        {

            var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            var tre = _DbContext.Tres.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName.ToLower());
            if (tre == null)
            {
                return BadRequest("User " + usersName + " doesn't have an tre");
            }


            var sub = _DbContext.Submissions.FirstOrDefault(x => x.TesId == tesId && x.Tre == tre);
            if (sub == null)
            {
                return BadRequest("Invalid tesid or tre not valid for tes");
            }

            UpdateSubmissionStatus.UpdateStatus(sub, statusType, description);
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

            var exch = _rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Main, "topic");

            _rabbit.Advanced.Publish(exch, RoutingConstants.Subs, false, new Message<int>(id));
            
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

        [HttpPost("UploadFile")]
        public BoolReturn UploadFile(IFormFile file, string bucketName)
        {
            if (file == null || file.Length == 0)
                return new BoolReturn() { Result = false };

            try
            {
                //var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                //Directory.CreateDirectory(uploadsFolder);

                //var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                //var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    file.CopyTo(stream);
                //}

                return new BoolReturn() { Result = true };
            }
            catch (Exception ex)
            {
                return new BoolReturn() { Result = false };
            }
        }

        [AllowAnonymous]
        [HttpGet("StageTypes")]
        public List<StageInfo> StageTypes()
        {
            var stage1List = new StageInfo();
            stage1List.stageName = "Submission Layer Validation";
            stage1List.stageNumber = 1;
            stage1List.statusTypeList = new List<StatusType>
            {
                StatusType.InvalidUser,
                StatusType.UserNotOnProject,
                StatusType.InvalidSubmission,
                StatusType.WaitingForCrateFormatCheck,
                StatusType.ValidatingUser,
                StatusType.ValidatingSubmission,
                StatusType.ValidationSuccessful
            };
            Dictionary<int, List<StatusType>> stage1Dict = new Dictionary<int, List<StatusType>>();
            stage1Dict.Add(1, stage1List.statusTypeList);
            stage1List.stagesDict = stage1Dict;

            var stage2List = new StageInfo();
            stage2List.stageName = "Tre Layer Validation";
            stage2List.stageNumber = 2;
            stage2List.statusTypeList = new List<StatusType>
            {
                StatusType.WaitingForAgentToTransfer,
                StatusType.TransferredToPod,
                StatusType.TRENotAuthorisedForProject,
                StatusType.AgentTransferringToPod,
                StatusType.TransferToPodFailed,
                StatusType.TRERejectedProject,
                StatusType.TREApprovedProject



            };
            Dictionary<int, List<StatusType>> stage2Dict = new Dictionary<int, List<StatusType>>();
            stage2Dict.Add(2, stage2List.statusTypeList);
            stage2List.stagesDict = stage2Dict;

            var stage3List = new StageInfo();
            stage3List.stageName = "Query Processing";
            stage3List.stageNumber = 3;
            stage3List.statusTypeList = new List<StatusType>
            {
                StatusType.WaitingForChildSubsToComplete,
                StatusType.PodProcessing,
                StatusType.PodProcessingComplete,
                StatusType.RequestCancellation,
                StatusType.CancellationRequestSent,
                StatusType.CancellingChildren,
                StatusType.Cancelled,
                StatusType.PodProcessingFailed

            };
            Dictionary<int, List<StatusType>> stage3Dict = new Dictionary<int, List<StatusType>>();
            stage3Dict.Add(3, stage3List.statusTypeList);
            stage3List.stagesDict = stage3Dict;

            var stage4List = new StageInfo();
            stage4List.stageName = "Data Approval Processing";
            stage4List.stageNumber = 4;
            stage4List.statusTypeList = new List<StatusType>
            {
                StatusType.DataOutApprovalBegun,
                StatusType.DataOutApprovalRejected,
                StatusType.DataOutApproved
            };
            Dictionary<int, List<StatusType>> stage4Dict = new Dictionary<int, List<StatusType>>();
            stage4Dict.Add(4, stage4List.statusTypeList);
            stage4List.stagesDict = stage4Dict;

            var stage5List = new StageInfo();
            stage5List.stageName = "Result";
            stage5List.stageNumber = 5;
            stage5List.statusTypeList = new List<StatusType>
            {
                StatusType.Cancelled,
                StatusType.Completed,
                StatusType.Running,
                StatusType.Failed
            };
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

            return infoList;

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
        //        StatusType.WaitingForCrateFormatCheck
        //    };
        //    return null;

        //}

        [HttpPost("SaveSubmissionFiles")]
        public IActionResult SaveSubmissionFiles(int submissionId, List<SubmissionFile> submissionFiles)
        {
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
    }
}
