using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using IdentityModel.Client;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using BL.Models;
using System.Data;
using System.Text.Json;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;



namespace DARE_FrontEnd.Controllers
{
    
    [AllowAnonymous]
    public class HomeController : Controller
    {
        
        

        public IActionResult Index()
        {
            return View();
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

