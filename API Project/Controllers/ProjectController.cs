using Microsoft.AspNetCore.Mvc;
using Project_Admin.Models;
using Project_Admin.Services.Project;

namespace API_Project.Controllers
{
    public class ProjectController : Controller
    {
        private IProjectsHandler _projectsHandler;

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("Save_Project")]

        public async Task<Projects> CreateProject(Projects Projects)
        {
            //add the model to the database using the function AddAsync in IdatasetMirrorSettingsHandler
            //CreateDatasetMirrorSettings(new DatasetMirrorSetting());
            await _projectsHandler.AddAsync(Projects);
            return Projects;
        }
    }
}
