using Microsoft.AspNetCore.Mvc;

namespace TRE_API.Controllers
{
    public class SubmissionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
