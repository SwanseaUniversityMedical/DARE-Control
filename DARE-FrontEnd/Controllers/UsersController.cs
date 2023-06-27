using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
