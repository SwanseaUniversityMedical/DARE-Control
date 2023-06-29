using Microsoft.AspNetCore.Mvc;

namespace TRE_API.Controllers
{
    public class MembershipController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
