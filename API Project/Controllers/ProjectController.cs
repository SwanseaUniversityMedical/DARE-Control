using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Project.Repositories.DbContexts;
using API_Project.Services.Project;
using BL.Models;

namespace API_Project.Controllers
{
    [Route("api/[controller]")]

    public class ProjectController : Controller
    {
        private readonly IProjectsHandler _projectsHandler;
        private readonly ApplicationDbContext _DbContext;
        //private readonly IProjectsHandler _ProjectService;
        //[HttpGet]

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

        public async Task<Projects> CreateProject(Projects Projects)
        {

            var existingProject = _DbContext.Projects.Find(Projects.Id);

            if (existingProject == null)
            {
                var id = 500;
                var newProject = await _projectsHandler.CreateProject(Projects);
            }

            await _projectsHandler.AddAsync(Projects);
            _DbContext.SaveChangesAsync();

            return Projects;
        }
    }
}
