using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using BL.Models;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace DARE_API.Controllers
{
    [Authorize]
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
        //[HttpPost("Save_Project")]

        //public async Task<Projects> CreateProject([FromBody] Projects Projects)
        //{
        //    Projects.StartDate = Projects.StartDate.ToUniversalTime();
        //    Projects.EndDate = Projects.EndDate.ToUniversalTime();
        //    _DbContext.Projects.Add(Projects);
        //    await _DbContext.SaveChangesAsync();

        //    return Projects;
        //}

        [HttpPost("Save_Project")]

        public async Task<Projects> CreateProject([FromBody] JsonObject project)
        {
            try
            {
                string jsonString = project.ToString();
                Projects projects = JsonConvert.DeserializeObject<Projects>(jsonString);

                //Projects projects = JsonConvert.DeserializeObject<Projects>(project);
                var model = new Projects();
                //2023-06-01 14:30:00 use this as the datetime
                model.Name = projects.Name;
                model.StartDate = projects.StartDate.ToUniversalTime();
                //model.Users = projects.Users.ToList();
                model.EndDate = projects.EndDate.ToUniversalTime();

                _DbContext.Projects.Add(model);

                await _DbContext.SaveChangesAsync();


                return model;
            }
            catch (Exception ex) { }

            return null;
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

        [HttpGet("Get_AllProjects")]

        public List<Projects> GetAllProjects()
        {
            var allProjects = _DbContext.Projects.ToList();
            
            foreach (var project in allProjects)
            {
                var id = project.Id;
                var name = project.Name;
            }
            //return returned.FirstOrDefault();
            return allProjects;
        }

    }
}
