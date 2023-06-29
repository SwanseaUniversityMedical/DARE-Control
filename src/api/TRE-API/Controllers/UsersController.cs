using Microsoft.AspNetCore.Mvc;

namespace TRE_API.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
