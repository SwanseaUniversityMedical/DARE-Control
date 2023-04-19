using Microsoft.AspNetCore.Mvc;

namespace Project_Admin.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetProjectDetails(int projectId)
        {
            try
            {
                return Ok();
                //This could be changed to get the information about the project
                //var details = _s3ProvisioningService.GetProjectDetails(projectId);
                //return Ok(details);
            }
            catch (Exception ex)
            {
               Console.Write(ex);
                return BadRequest();
            }
        }
    }
}
