﻿using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using BL.Models.ViewModels;
using DARE_API.Services.Contract;
using Microsoft.AspNetCore.Authentication;
using DARE_API.Services;
using BL.Models.Tes;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DARE_API.Controllers
{

    
    
    
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        
        private readonly ApplicationDbContext _DbContext;
        private readonly IKeycloakMinioUserService _keycloakMinioUserService;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(ApplicationDbContext applicationDbContext, IKeycloakMinioUserService keycloakMinioUserService, IHttpContextAccessor httpContextAccessor)
        {

            _DbContext = applicationDbContext;
            _keycloakMinioUserService = keycloakMinioUserService;
            _httpContextAccessor = httpContextAccessor;

        }

        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("SaveUser")]
        public async Task<User> SaveUser([FromBody] FormData data) 
        {
            try
            {

                User userData = JsonConvert.DeserializeObject<User>(data.FormIoString);
                userData.Name = userData.Name.Trim();
                userData.Email = userData.Email.Trim();
                userData.FormData = data.FormIoString;


                if (_DbContext.Users.Any(x => x.Name.ToLower() == userData.Name.ToLower().Trim() && x.Id != userData.Id))
                {
                    
                    return new User() { Error = true, ErrorMessage = "Another user already exists with the same name" };
                }

                var logtype = LogType.AddUser;

                if (userData.Id > 0)
                {
                    if (_DbContext.Users.Select(x => x.Id == userData.Id).Any())
                    {
                        _DbContext.Users.Update(userData);
                        logtype = LogType.AddUser;
                    }
                    else
                    {
                        _DbContext.Users.Add(userData);
                    }
                }
                else
                    _DbContext.Users.Add(userData);


                await _DbContext.SaveChangesAsync();

                
                await ControllerHelpers.AddAuditLog(logtype, userData, null, null, null, null, _httpContextAccessor, User, _DbContext);

                return userData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "SaveUser");
                return new User(); ;
                throw;
            }

            
        }


        [AllowAnonymous]
        [HttpGet("GetUser")]
        public User? GetUser(int userId)
        {
            try
            {
                var returned = _DbContext.Users.Find(userId);
                if (returned == null)
                {
                    return null;
                }
                Log.Information("{Function} User retrieved successfully", "GetUser");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetUser");
                throw;
            }

            
        }


        [AllowAnonymous]
        [HttpGet("GetAllUsers")]
        public List<User> GetAllUsers()
        {
            try
            {
                var allUsers = _DbContext.Users.ToList();

                
            
               Log.Information("{Function} Users retrieved successfully", "GetAllUsers");
                return allUsers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "GetAllUsers");
                throw;
            }

            
        }
       
        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("AddProjectMembership")]
        public async Task<ProjectUser?> AddProjectMembership([FromBody]ProjectUser model)
        {
            try
            {
                var user = _DbContext.Users.FirstOrDefault(x => x.Id == model.UserId);
                if (user == null)
                {
                    Log.Error("{Function} Invalid user id {UserId}", "AddProjectMembership", model.UserId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "AddProjectMembership", model.ProjectId);
                    return null;
                }

               
                if (user.Projects.Any(x => x == project))
                {
                    Log.Error("{Function} User {UserName} is already on {ProjectName}", "AddProjectMembership", user.Name, project.Name);
                    return null;
                }
                user.Projects.Add(project);

                await _DbContext.SaveChangesAsync();
                await ControllerHelpers.AddUserToMinioBucket(user, project, _httpContextAccessor, "policy", _keycloakMinioUserService, User, _DbContext);
                await ControllerHelpers.AddAuditLog(LogType.AddUserToProject, user, project, null, null, null, _httpContextAccessor, User, _DbContext);
                
                Log.Information("{Function} Added User {UserName} to {ProjectName}", "AddProjectMembership", user.Name, project.Name);

                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddProjectMembership");
                throw;
            }


        }
        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("RemoveProjectMembership")]
        public async Task<ProjectUser?> RemoveProjectMembership([FromBody] ProjectUser model)
        {
            try
            {
                var user = _DbContext.Users.FirstOrDefault(x => x.Id == model.UserId);
                if (user == null)
                {
                    Log.Error("{Function} Invalid user id {UserId}", "RemoveProjectMembership", model.UserId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "RemoveProjectMembership", model.ProjectId);
                    return null;
                }
              
                if (!user.Projects.Any(x => x == project))
                {
                    Log.Error("{Function} User {UserName} is not in the {ProjectName}", "RemoveProjectMembership", user.Name, project.Name);
                    return null;
                }
                user.Projects.Remove(project);
                await _DbContext.SaveChangesAsync();
                await ControllerHelpers.RemoveUserFromMinioBucket(user, project, _httpContextAccessor, "policy", _keycloakMinioUserService, User, _DbContext);
                await ControllerHelpers.AddAuditLog(LogType.RemoveUserFromProject, user, project, null, null, null, _httpContextAccessor, User, _DbContext);
                
                Log.Information("{Function} Added Project {ProjectName} to {UserName}", "RemoveProjectMembership", project.Name, user.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "RemoveUserMembership");
                throw;
            }


        }
    }
}
