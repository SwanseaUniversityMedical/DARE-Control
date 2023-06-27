using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Controllers
{
    public class EndpointsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
