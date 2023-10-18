
using BL.Models.APISimpleTypeReturns;
using BL.Models;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using BL.Services;
using BL.Models.ViewModels;
using NuGet.Protocol;
using System.Linq;

namespace DARE_FrontEnd.Controllers
{

    [AllowAnonymous]
    public class HomeController : Controller
    {

        private readonly IDareClientHelper _clientHelper;
        private readonly IConfiguration _configuration;

        public HomeController(IDareClientHelper client, IConfiguration configuration)
        {
            _clientHelper = client;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var getAllProj = _clientHelper.CallAPIWithoutModel< List<Project>>("/api/Project/GetAllProjects").Result;
            ViewBag.getAllProj = getAllProj.Count;

            var getAllSubs = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions").Result;
            ViewBag.getAllSubs = getAllSubs.Count;

            var getAllUsers = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers").Result;
            ViewBag.getAllUsers = getAllUsers.Count;

            var getAllTres = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres").Result;
            ViewBag.getAllTres = getAllTres.Count;

            return View();
        }

        [Authorize]
        public IActionResult LoggedInUser()
        {
            var getAllProj = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects").Result;
            ViewBag.getAllProj = getAllProj;

            var getAllSubs = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions").Result;
            ViewBag.getAllSubs = getAllSubs.Count;

            var getAllUsers = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers").Result;
            ViewBag.getAllUsers = getAllUsers.Count;

            var getAllTres = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres").Result;
            ViewBag.getAllTres = getAllTres.Count;
            var userOnProjList = new List<User>();
            var projectList = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects").Result.ToList();
            foreach (var proj in projectList)
            {
                foreach (var user in proj.Users)
                {
                    if (user.Name == "luke.young")

                        userOnProjList.Add(user);
                    else { 
                    }
                }
                
            }
            var userOnProjectsCount = userOnProjList.ToList().Count;
            ViewBag.userOnProjectCount = userOnProjectsCount;
                
            var userModel = new User
            {
                Name = User.Identity.Name,
                
                Projects = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects").Result,

                Submissions = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions").Result,
        };
            
            return View(userModel);
        }

        public IActionResult TermsAndConditions()
        {
            return View();
        }
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
    }

}

