
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

            var getAllProj = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects").Result;
            ViewBag.getAllProj = getAllProj.Count;

            var getAllSubs = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions").Result.Where(x => x.Parent == null).ToList();
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
            if(User.Identity.IsAuthenticated == false) {
                return RedirectToAction("Index", "Home");
            }
            var preferedUsername = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            
            var getAllProj = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects").Result;
            ViewBag.getAllProj = getAllProj;

            var getAllSubs = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions").Result.Where(x => x.Parent == null).ToList();
            ViewBag.getAllSubs = getAllSubs.Count;

            var getAllUsers = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers").Result;
            ViewBag.getAllUsers = getAllUsers.Count;

            var getAllTres = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres").Result;
            ViewBag.getAllTres = getAllTres.Count;

            var userOnProjList = new List<User>();
            var userOnProjListProj = new List<Project>();
            var projectList = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects").Result.ToList();
            foreach (var proj in projectList)
            {
                foreach (var user in proj.Users)
                {
                    if (user.Name == preferedUsername)
                    {
                        userOnProjListProj.Add(proj);

                        userOnProjList.Add(user);
                    }
                }
            }
            var userOnProjectsCount = userOnProjList.ToList().Count;
            ViewBag.userOnProjectCount = userOnProjectsCount;

            var userWroteSubList = new List<User>();
            var userWroteSubListSub = new List<Submission>();
            var subList = getAllSubs;// _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions").Result.ToList();
            foreach (var sub in subList)
            {

                    if (sub.SubmittedBy.Name == preferedUsername)
                {
                    userWroteSubListSub.Add(sub);
                    userWroteSubList.Add(sub.SubmittedBy);
                
                }

            }
            var userWroteSubCount = userWroteSubList.ToList().Count;
            var distintProj = userOnProjListProj.Distinct();
            var distinctSub = userWroteSubListSub.Distinct();
            ViewBag.userWroteSubCount = userWroteSubCount;

            var userModel = new User
            {
                Name = User.Identity.Name,

                Projects = distintProj.ToList(),

                Submissions = distinctSub.ToList(),
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

