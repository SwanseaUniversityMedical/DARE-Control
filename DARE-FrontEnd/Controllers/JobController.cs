using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Controllers
{
    public class JobController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
