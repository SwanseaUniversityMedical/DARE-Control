using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Controllers
{
    public class JobsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
