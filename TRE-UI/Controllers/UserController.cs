using Microsoft.AspNetCore.Mvc;

namespace TRE_UI.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
