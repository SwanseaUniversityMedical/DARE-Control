using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TRE_UI.Services;
using BL.Services;

namespace TRE_UI.Controllers
{
    /// <summary>
    /// Controller for DMN rule management UI
    /// </summary>
    [Authorize(Roles = "dare-tre-admin")]
    public class DmnController : Controller
    {
        private readonly ITREClientHelper _clientHelper;
        private readonly ILogger<DmnController> _logger;

        public DmnController(ITREClientHelper clientHelper, ILogger<DmnController> logger)
        {
            _clientHelper = clientHelper;
            _logger = logger;
        }

        /// <summary>
        /// Display the DMN rule management page
        /// </summary>
        /// <returns>View with DMN editor</returns>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading DMN management page");
                return View("Error");
            }
        }

        /// <summary>
        /// Test DMN evaluation page
        /// </summary>
        /// <returns>View for testing DMN rules</returns>
        //[HttpGet]
        //public IActionResult Test()
        //{
        //    try
        //    {
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading DMN test page");
        //        return View("Error");
        //    }
        //}
    }
}
