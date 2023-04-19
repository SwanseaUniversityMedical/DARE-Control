using Microsoft.AspNetCore.Mvc;

namespace Project_Admin.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
