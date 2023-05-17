using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using API_Project.Services.Project;
using BL.Models;
using BL.Repositories.DbContexts;

namespace API_Project.Controllers
{
    [Route("api/[controller]")]

    public class ProjectController : Controller
    {
       // private readonly IProjectsHandler _projectsHandler;
        private readonly ApplicationDbContext _DbContext;
        //private readonly IProjectsHandler _ProjectService;
        //[HttpGet]
        public ProjectController(ApplicationDbContext applicationDbContext)
        {
            
            _DbContext = applicationDbContext;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet("HelloWorld")]

        public IActionResult HelloWorld()
        {
            return Ok();
        }
        [HttpPost("Save_Project")]

        public async Task<Projects> CreateProject([FromBody] Projects Projects)
        {
            Projects.StartDate = Projects.StartDate.ToUniversalTime();
            Projects.EndDate = Projects.EndDate.ToUniversalTime();
            //var existingProject = _DbContext.Projects.Find(Projects.Id);

            //if (existingProject == null)
            //{
            //    var id = 500;
            //    var newProject = await _projectsHandler.CreateProject(Projects);
            //}
            ////var newProject = await _projectsHandler.CreateProject(Projects);
            ////await _projectsHandler.AddAsync(newProject);
            _DbContext.Projects.Add(Projects);
            await _DbContext.SaveChangesAsync();

            return Projects;
        }
    }
}
