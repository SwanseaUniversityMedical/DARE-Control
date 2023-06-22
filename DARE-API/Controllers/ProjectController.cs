using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using BL.Models;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using DARE_API.Controllers;
using static BL.Controllers.UserController;
using Minio.DataModel;
using Serilog;
using DARE_API.Services.Contract;
using DARE_API.Models;

namespace DARE_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class ProjectController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly MinioSettings _minioSettings;
        private readonly IMinioService _minioService;


        public ProjectController(ApplicationDbContext applicationDbContext, MinioSettings minioSettings, IMinioService minioService)
        {

            _DbContext = applicationDbContext;
            _minioSettings = minioSettings;
            _minioService = minioService;
        }


        [HttpGet("HelloWorld")]

        public IActionResult HelloWorld()
        {
            return Ok();
        }


        //[HttpPost("Save_Project")]

        //public async Task<Projects> CreateProject([FromBody] JsonObject project)
        //{
        //    try
        //    {
        //        string jsonString = project.ToString();
        //        Projects projects = JsonConvert.DeserializeObject<Projects>(jsonString);

        //        //Projects projects = JsonConvert.DeserializeObject<Projects>(project);
        //        var model = new Projects();
        //        //2023-06-01 14:30:00 use this as the datetime
        //        model.Name = projects.Name;
        //        model.StartDate = projects.StartDate.ToUniversalTime();
        //        //model.Users = projects.Users.ToList();
        //        model.EndDate = projects.EndDate.ToUniversalTime();

        //        _DbContext.Projects.Add(model);

        //        await _DbContext.SaveChangesAsync();


        //        return model;
        //    }
        //    catch (Exception ex) { }

        //    return null;
        //}

        [HttpPost("Save_Project")]

        public async Task<Projects> CreateProject(data data)
        {
            //var backet = "testbucket2";
            //var bucketExists2 = await _minioService.CreateBucket(_minioSettings, backet);
            //if (!bucketExists2)
            //{
            //    Log.Error("S3GetListObjects: Failed to create bucket {name}.", backet);
            //}

            try
            {
                Projects projects = JsonConvert.DeserializeObject<Projects>(data.FormIoString);

                //Projects projects = JsonConvert.DeserializeObject<Projects>(project);
                var model = new Projects();
                //2023-06-01 14:30:00 use this as the datetime
                model.Name = projects.Name;
                model.StartDate = projects.StartDate.ToUniversalTime();
                //model.Users = projects.Users.ToList();
                model.EndDate = projects.EndDate.ToUniversalTime();

                var bucketExists = await _minioService.CreateBucket(_minioSettings, model.Name);
                if (!bucketExists)
                {
                    Log.Error("S3GetListObjects: Failed to create bucket {name}.", model.Name);
                }

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

        [HttpGet("Get_AllMemberships")]

        public List<ProjectMembership> GetAllMemberships()
        {
            var allProjectMemberships = _DbContext.ProjectMemberships.ToList();

            //foreach (var projectMembership  in allProjectMemberships)
            //{
            //    var id = projectMembership.Id;
            //}
            //return returned.FirstOrDefault();
            return allProjectMemberships;
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
