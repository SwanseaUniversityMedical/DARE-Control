using Data_Egress_UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BL.Services;
using BL.Models;
using EasyNetQ.Management.Client.Model;
using System.Collections.Generic;
using BL.Models.APISimpleTypeReturns;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Amazon.S3.Model;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using BL.Models.Enums;
using Microsoft.AspNetCore.StaticFiles;

namespace Data_Egress_UI.Controllers
{
    [Authorize(Roles = "data-egress-admin")]
    public class DataController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataEgressClientHelper _dataClientHelper;

        public DataController(ILogger<HomeController> logger, IDataEgressClientHelper datahelper)
        {
            _logger = logger;
            _dataClientHelper = datahelper;
        }
   
        [HttpGet]
        public IActionResult GetAllUnprocessedEgresses()
        {
            var unprocessedfiles = _dataClientHelper.CallAPIWithoutModel<List<EgressSubmission>>("/api/DataEgress/GetAllUnprocessedEgresses/").Result;
           
            return View(unprocessedfiles);

        }

        [HttpGet]
        public IActionResult GetEgress(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("id", id.ToString());

            var files = _dataClientHelper.CallAPIWithoutModel<EgressSubmission>("/api/DataEgress/GetEgress/", paramlist).Result;

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

            return RedirectToAction("GetAllUnprocessedEgresses");
            
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

            var egressFile = _dataClientHelper.CallAPIWithoutModel<EgressFile>("/api/DataEgress/GetEgressFile", paramlist).Result;
            var file = _dataClientHelper.CallAPIToGetFile(
                "/api/DataEgress/DownloadFile", paramlist).Result;
            return  File(file, GetContentType(egressFile.Name), egressFile.Name);
        }

        [HttpGet]
        public IActionResult GetAllEgresses()
        {
            var egresses = _dataClientHelper.CallAPIWithoutModel<List<EgressSubmission>>("/api/DataEgress/GetAllEgresses/").Result;

           

           
            return View(egresses);
        }



    }
}