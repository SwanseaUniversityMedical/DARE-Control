using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Controllers
{
    public class ProjectsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
