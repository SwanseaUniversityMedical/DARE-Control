using Microsoft.AspNetCore.Mvc;

namespace DARE_API.Controllers
{
    public class AuditController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
