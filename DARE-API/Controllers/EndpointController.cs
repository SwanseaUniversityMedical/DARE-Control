using Microsoft.AspNetCore.Mvc;

namespace DARE_API.Controllers
{
    public class EndpointController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
