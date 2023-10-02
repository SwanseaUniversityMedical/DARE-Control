using Data_Egress_UI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
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

namespace Data_Egress_UI.Controllers
{
    //[Authorize(Roles = "data-egress-admin")]
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
        public IActionResult GetAllFiles()
        {

            var files = _dataClientHelper.CallAPIWithoutModel<List<DataFiles>>("/api/DataEgress/GetAllFiles/").Result;
            return View(files);


        }
                   
       
        [HttpGet]
        public IActionResult GetAllUnprocessedFiles()
        {

            var unprocessedfiles = _dataClientHelper.CallAPIWithoutModel<List<DataFiles>>("/api/DataEgress/GetAllUnprocessedFiles/").Result;
            return View(unprocessedfiles);


        }
        [HttpPost]
        public async Task<IActionResult> EditFileData(DataFiles model)
        {
            var result =
                await _dataClientHelper.CallAPI<List<DataFiles>, List<DataFiles>>("/api/DataEgress/UpdateFileData", new List<DataFiles>() { model });

            return View(result.First());
        }

        [HttpGet]
        public IActionResult DownloadFile(int? FileId)
        {
          
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("FileId", FileId.ToString());
            var file = _dataClientHelper.CallAPIWithoutModel<DataFiles>(
                "/api/DataEgress/DownloadFile/", paramlist).Result;
            return View(file);
        }

    }
}