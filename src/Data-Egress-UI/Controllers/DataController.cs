using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BL.Services;
using BL.Models;
using Microsoft.AspNetCore.StaticFiles;


namespace Data_Egress_UI.Controllers
{
    [Authorize(Roles = "data-egress-admin")]
    public class DataController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataEgressClientHelper _dataClientHelper;
        private readonly IConfiguration _configuration;

        public DataController(ILogger<HomeController> logger, IDataEgressClientHelper datahelper, IConfiguration configuration)
        {
            _logger = logger;
            _dataClientHelper = datahelper;
            _configuration = configuration;
        }
   
        [HttpGet]
        public IActionResult GetAllUnprocessedEgresses()
        {
            var paramlist = new Dictionary<string, string>();

            paramlist.Add("unprocessedonly", true.ToString());
            var unprocessedfiles = _dataClientHelper.CallAPIWithoutModel<List<EgressSubmission>>("/api/DataEgress/GetAllEgresses/", paramlist).Result;
           
            return View(unprocessedfiles);

        }

        [HttpGet]
        public IActionResult GetEgress(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("id", id.ToString());

            var files = _dataClientHelper.CallAPIWithoutModel<EgressSubmission>("/api/DataEgress/GetEgress/", paramlist).Result;
            // Get S3 settings from configuration
            var s3Url = _configuration["S3Settings:Url"] ?? "http://localhost:9003";
            var s3BucketPath = _configuration["S3Settings:BucketPath"] ?? "/rustfs/console/browser/?bucket=";
            ViewBag.S3Url = s3Url;
            ViewBag.S3BucketPath = s3BucketPath;
            
            return View(files);
        }

        [HttpGet]
        public IActionResult EditFileData(int Id , int Status)
        {          
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("id", Id.ToString());
            paramlist.Add("Status", Status.ToString());
            var files = _dataClientHelper.CallAPIWithoutModel<DataFiles>("/api/DataEgress/UpdateFileData/", paramlist).Result;
            return RedirectToAction("GetFiles", new { Id = Id });
        }
        [HttpPost]
        public IActionResult GetEgress(EgressSubmission model, string submitButton)
        {
            if (submitButton == "SubmitButton")
            {
                var egress = _dataClientHelper.CallAPI<EgressSubmission, EgressSubmission>("/api/DataEgress/CompleteEgress/", model).Result;

            }
            else if (submitButton == "SaveButton")
            {
                var egress = _dataClientHelper.CallAPI<EgressSubmission, EgressSubmission>("/api/DataEgress/PartialEgress/", model).Result;

            }

            return RedirectToAction("GetAllEgresses", "Data", new { unprocessedonly = true });

        }
        public static string GetContentType(string fileName)
        {
            // Create a new FileExtensionContentTypeProvider
            var provider = new FileExtensionContentTypeProvider();

            // Try to get the content type based on the file name's extension
            if (provider.TryGetContentType(fileName, out var contentType))
            {
                return contentType;
            }

            // If the content type cannot be determined, provide a default value
            return "application/octet-stream"; // This is a common default for unknown file types
        }

        [HttpGet]
        public IActionResult DownloadFile(int? fileId)
        {

            var paramlist = new Dictionary<string, string>
            {
                { "id", fileId.ToString() }
            };

            var egressFile = _dataClientHelper.CallAPIWithoutModel<EgressFile>($"/api/DataEgress/GetEgressFile/{fileId}").Result;
            var file = _dataClientHelper.CallAPIToGetFile(
                "/api/DataEgress/DownloadFile", paramlist).Result;
            return  File(file, GetContentType(egressFile.Name), egressFile.Name);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllEgressesuUnprocessed()
        {

            var paramlist = new Dictionary<string, string>();
          
            ViewBag.PageTitle = "Unprocessed Egresses";
            paramlist.Add("unprocessedonly", true.ToString());

        
            List<EgressSubmission>? egresses  = await _dataClientHelper.CallAPIWithoutModel<List<EgressSubmission>>("/api/DataEgress/GetAllEgresses/", paramlist);

            return View(egresses);
        }

        [HttpGet]
        public IActionResult GetAllEgresses()
        {

            var paramlist = new Dictionary<string, string>();

            ViewBag.PageTitle = "All Egresses";
            paramlist.Add("unprocessedonly", false.ToString());

            List<EgressSubmission> egresses = _dataClientHelper
                .CallAPIWithoutModel<List<EgressSubmission>>("/api/DataEgress/GetAllEgresses/", paramlist).Result;

            return View(egresses);
        }



    }
}