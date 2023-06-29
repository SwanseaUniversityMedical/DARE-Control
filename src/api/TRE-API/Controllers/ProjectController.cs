using Microsoft.AspNetCore.Mvc;

namespace TRE_API.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
