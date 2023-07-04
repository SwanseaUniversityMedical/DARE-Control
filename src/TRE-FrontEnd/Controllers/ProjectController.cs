using Microsoft.AspNetCore.Mvc;

namespace TRE_FrontEnd.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
