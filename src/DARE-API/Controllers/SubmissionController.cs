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
    }
}
