using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Project_Admin.Controllers
{
    [Authorize]
    public class LoginController : Controller
    {
       
        public IActionResult Index()
        {
            return View();
        }
    }
}
