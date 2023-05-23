using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Controllers
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
