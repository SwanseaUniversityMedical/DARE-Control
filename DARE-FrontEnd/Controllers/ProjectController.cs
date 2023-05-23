using Microsoft.AspNetCore.Mvc;
using DARE_FrontEnd.Models;
using DARE_FrontEnd.Services.Project;

namespace Project_Admin.Controllers
{

    public class ProjectController : Controller
    {

        private IProjectsHandler _projectsHandler;


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

        public ProjectController(IProjectsHandler projectsHandler)
        {
            _projectsHandler = projectsHandler;
        }



        //seems to go to this function when entering the URL
        //[HttpPost("CreateDatasetMirrorSettings")]
        //[HttpPost("Save_Mirroring")]

        //public async Task<ProjectModel> CreateDatasetMirrorSettings(ProjectModel ProjectModel)
        //{
        //    //add the model to the database using the function AddAsync in IdatasetMirrorSettingsHandler
        //    //CreateDatasetMirrorSettings(new DatasetMirrorSetting());
        //    await _projectsHandler.AddAsync(ProjectModel);
        //    return ProjectModel;
        //}
    }
}
