using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using BL.Models;


namespace DARE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProjectController : ControllerBase
    {
       
        private readonly ApplicationDbContext _DbContext;
       
       
        public ProjectController(ApplicationDbContext applicationDbContext)
        {
            
            _DbContext = applicationDbContext;
        }
       

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
            _DbContext.Projects.Add(Projects);
            await _DbContext.SaveChangesAsync();

            return Projects;
        }


        [HttpPost("Add_Membership")]

        public async Task<ProjectMembership> AddMembership(int userid, int projectid)
        {

            var membership = new ProjectMembership();
           //var theuser =
            
            membership.Users = await _DbContext.Users.SingleAsync(x => x.Id == userid);
            membership.Projects = await _DbContext.Projects.SingleAsync(x => x.Id == projectid);

            //membership.Id = 1;
            _DbContext.ProjectMemberships.Add(membership);
            await _DbContext.SaveChangesAsync();
            




            return membership;
        }



        [HttpGet("Get_Project/{projectId}")]

        public Projects GetProject(int projectId)
        {
            var returned = _DbContext.Projects.Find(projectId);
            if (returned == null)
            {
                return null;
            }
            //return returned.FirstOrDefault();
            return returned;
        }

       

    }
}
