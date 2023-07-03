using Microsoft.AspNetCore.Mvc;

namespace TRE_UI.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
