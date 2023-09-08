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

        

        public SubmissionController(ApplicationDbContext repository, IBus rabbit)
        {
            _DbContext = repository;
            

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

            var results = tre.Submissions.Where(x =>x.Status == StatusType.WaitingForAgentToTransfer).ToList();
            

            return StatusCode(200, results);
        }


        [HttpGet]
        [Route("UpdateStatusForTre")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public  IActionResult UpdateStatusForTre(string tesId, StatusType statusType, string? description)
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


            return StatusCode(200, new APIReturn(){ReturnType = ReturnType.voidReturn});
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
        [HttpGet("GetASubmission")]
        public Submission GetASubmission(int submissionId)
        {
            try
            {

                var Submission = _DbContext.Submissions.Where(x => x.Id == submissionId).FirstOrDefault();

                Log.Information("{Function} Submission retrieved successfully", "GetASubmission");
                return Submission;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetASubmission");
                throw;
            }
        }
        public static StatusType StatusTypes(StatusType[] listofStatus) {
            //var stage1 = "validation";
            //var stage2 = "processing";
            //var stage3 = "result";
            //List<string> stages = new List<string>();
            //stages.Add(stage1);
            //stages.Add(stage2);
            //stages.Add(stage3);


        //List<List<StatusType>> myList = new List<List<StatusType>>();
        List<StatusType> stage1List = new List<StatusType>();
            stage1List.Add(StatusType.WaitingForChildSubsToComplete);
            stage1List.Add(StatusType.WaitingForAgentToTransfer);
            stage1List.Add(StatusType.WaitingForCrateFormatCheck);

            List<StatusType> stage2List = new List<StatusType>();
            stage2List.Add(StatusType.PodProcessing);
            stage2List.Add(StatusType.DataOutApprovalBegun);
            stage2List.Add(StatusType.TransferredToPod);

            List<StatusType> stage3List = new List<StatusType>();
            stage3List.Add(StatusType.DataOutApprovalRejected);
            stage3List.Add(StatusType.UserNotOnProject);
            stage3List.Add(StatusType.InvalidSubmission);
            stage3List.Add(StatusType.InvalidUser);
            stage3List.Add(StatusType.TRENotAuthorisedForProject);
            stage3List.Add(StatusType.CancellingChildren);
            stage3List.Add(StatusType.Cancelled);

            List<StatusType> stage4List = new List<StatusType>();
            stage4List.Add(StatusType.PodProcessingComplete);
            stage4List.Add(StatusType.DataOutApproved);
            stage4List.Add(StatusType.Completed);
            stage4List.Add(StatusType.DataOutApprovalRejected);


            Dictionary<string, List<StatusType>> stages = new Dictionary<string, List<StatusType>>();
            //just incase use this if i need to make the list of stage4List a list of a list e.g adding in TRE name or a colour
            //Dictionary<string, List<List<StatusType>>> stages = new Dictionary<string, List<List<StatusType>>>();

            stages.Add("stage1", stage1List);
            stages.Add("stage2", stage2List);
            stages.Add("stage3", stage3List);
            stages.Add("stage4", stage4List);

            return 0;

        }
        public static List<StageInfo> StageTypes()
        {
            var stage1List = new StageInfo();
            stage1List.stageName = "Submission Layer Validation";
            stage1List.stageNumber = 1;
            stage1List.statusTypeList = new List<StatusType>
            {
                StatusType.InvalidUser ,
                StatusType.UserNotOnProject ,
                StatusType.InvalidSubmission,
                StatusType.WaitingForCrateFormatCheck
            };

            var stage2List = new StageInfo();
            stage2List.stageName = "Tre Layer Validation";
            stage2List.stageNumber = 2;
            stage2List.statusTypeList = new List<StatusType>
            {
                StatusType.WaitingForAgentToTransfer,
                StatusType.TransferredToPod,
                StatusType.TRENotAuthorisedForProject
            };

            var stage3List = new StageInfo();
            stage3List.stageName = "Query Processing";
            stage3List.stageNumber = 3;
            stage3List.statusTypeList = new List<StatusType>
            {
                StatusType.WaitingForChildSubsToComplete,
                StatusType.PodProcessing,
                StatusType.RequestCancellation

            };

            var stage4List = new StageInfo();
            stage4List.stageName = "Data Approval Processing";
            stage4List.stageNumber = 4;
            stage4List.statusTypeList = new List<StatusType>
            {
                StatusType.DataOutApprovalBegun,
                StatusType.CancellationRequestSent,
                StatusType.CancellingChildren
            };

            var stage5List = new StageInfo();
            stage5List.stageName = "Result";
            stage5List.stageNumber = 5;
            stage5List.statusTypeList = new List<StatusType>
            {
                StatusType.PodProcessingComplete,
                StatusType.DataOutApprovalRejected,
                StatusType.DataOutApproved,
                StatusType.Completed,
                StatusType.Cancelled
            };

            List<StageInfo> infoList = new List<StageInfo>();
            // var infoList = new List<List<StageInfo>>();
            infoList.Add(stage1List);
            infoList.Add(stage2List);
            infoList.Add(stage3List);
            infoList.Add(stage4List);
            infoList.Add(stage5List);


            return infoList;

        }
    }
}
