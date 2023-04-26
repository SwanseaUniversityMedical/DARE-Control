using Microsoft.AspNetCore.Mvc;

namespace API_Project.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
