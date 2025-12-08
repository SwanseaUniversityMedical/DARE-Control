using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IConfiguration _configuration;

        public DmnController(
            ITREClientHelper clientHelper,
            ILogger<DmnController> logger,
            IConfiguration configuration)
        {
            _clientHelper = clientHelper;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Display the DMN rule management page
        /// </summary>
        /// <returns>View with DMN editor</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Access token not found for user: {User}", User?.Identity?.Name);
                }
                else
                {
                    _logger.LogInformation("Access token retrieved for user: {User}", User?.Identity?.Name);
                }

                ViewBag.AccessToken = accessToken;
                ViewBag.ApiBaseUrl = _configuration["TreAPISettings:PublicApiBaseUrl"];

                _logger.LogInformation("DMN management page loaded for user: {User}", User?.Identity?.Name);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading DMN management page");
                return View("Error");
            }
        }

        /// <summary>
        /// Retrieves the Keycloak access token from the current user's authentication ticket
        /// </summary>
        /// <returns>Access token string, or empty string if not found</returns>
        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync();

                if (result?.Principal == null)
                {
                    _logger.LogWarning("No authentication principal found");
                    return string.Empty;
                }

                if (result.Properties?.GetTokenValue("access_token") is string token && !string.IsNullOrEmpty(token))
                {
                    _logger.LogDebug("Access token retrieved from authentication result");
                    return token;
                }

                _logger.LogWarning("Access token not found in authentication result");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving access token");
                return string.Empty;
            }
        }


        /// <summary>
        /// Test DMN evaluation page (currently commented out)
        /// </summary>
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