using Microsoft.AspNetCore.Mvc;

namespace TRE_FrontEnd.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
