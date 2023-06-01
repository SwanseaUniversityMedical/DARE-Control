using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using API_Project.Services.Project;
using BL.Models;
using BL.Repositories.DbContexts;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;

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
        //[HttpPost("Save_Project")]

        //public async Task<Projects> CreateProject([FromBody] Projects Projects)
        //{
        //    Projects.StartDate = Projects.StartDate.ToUniversalTime();
        //    Projects.EndDate = Projects.EndDate.ToUniversalTime();
        //    //var existingProject = _DbContext.Projects.Find(Projects.Id);

        //    //if (existingProject == null)
        //    //{
        //    //    var id = 500;
        //    //    var newProject = await _projectsHandler.CreateProject(Projects);
        //    //}
        //    ////var newProject = await _projectsHandler.CreateProject(Projects);
        //    ////await _projectsHandler.AddAsync(newProject);
        //    _DbContext.Projects.Add(Projects);
        //    await _DbContext.SaveChangesAsync();

        //    return Projects;
        //}
        [HttpPost("Save_Project")]

        public async Task<Projects> CreateProject([FromBody] JsonObject project)
        {
            try {
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
            //Console.WriteLine($"Name: {users.Name}");

            //model.Users = projects.Users;


            //string jsonString = Projects.ToString();
            //JObject jsonObject = JObject.Parse(jsonString);
            //var projectName = (string)jsonObject["ProjectName"];
           // var startDate = (int)jsonObject["StartDate"];
           // var endDate = (string)jsonObject["EndDate"];

            //var model = new Projects();
            //model.Id = 5;
            //model.StartDate = startDate;
            //model.EndDate = DateTime.Now;
            //model.Name = projectName;
            //model.Users = new List<User>();

            //_DbContext.Projects.Add(model);

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
            //try
            //{
            //    _DbContext.ProjectMemberships.Add(membership);
            //    await _DbContext.SaveChangesAsync();
            //}
            //catch (Exception ex)
            //{
            //    // Display the details of the outer exception
            //    Console.WriteLine("Outer Exception Type: " + ex.GetType().Name);
            //    Console.WriteLine("Outer Exception Message: " + ex.Message);

            //    // Check if there is an inner exception
            //    if (ex.InnerException != null)
            //    {
            //        // Display the details of the inner exception
            //        Console.WriteLine("Inner Exception Type: " + ex.InnerException.GetType().Name);
            //        Console.WriteLine("Inner Exception Message: " + ex.InnerException.Message);
            //        // You can continue to access inner exceptions if there are multiple levels
            //    }
            //    }




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

        



        //[HttpPost("Get_Project")]

        //public async Task<Projects> GetProject(int projectsId)
        //{

        //    //var existingProject = _DbContext.Projects.Find(Projects.Id);

        //    //if (existingProject == null)
        //    //{
        //    //    var id = 500;
        //    //    var newProject = await _projectsHandler.CreateProject(Projects);
        //    //}
        //    ////var newProject = await _projectsHandler.CreateProject(Projects);
        //    ////await _projectsHandler.AddAsync(newProject);
        //    _DbContext.Projects.Add(Projects);
        //    await _DbContext.SaveChangesAsync();

        //    return Projects;
        //}

    }
}
