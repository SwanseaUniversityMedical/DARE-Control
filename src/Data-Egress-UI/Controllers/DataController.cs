﻿using Data_Egress_UI.Models;
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
using Amazon.S3.Model;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using BL.Models.Enums;

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
        public IActionResult GetAllUnprocessedFiles()
        {
            var unprocessedfiles = _dataClientHelper.CallAPIWithoutModel<List<DataFiles>>("/api/DataEgress/GetAllUnprocessedFiles/").Result;
            var filecount = 0;
            var submissionIDCounts = unprocessedfiles
            .GroupBy(file => file.SubmissionId)
            .Select(group => new { submissionId = group.Key, Count = group.Count() })
            .ToList();
            foreach (var count in submissionIDCounts)
            {
                filecount = count.Count;
            }

            ViewBag.Filecount = filecount;
            return View(unprocessedfiles);

        }

        [HttpGet]
        public IActionResult GetFiles(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("id", id.ToString());

            var files = _dataClientHelper.CallAPIWithoutModel<List<DataFiles>>("/api/DataEgress/GetFilesBySubmissionId/", paramlist).Result;

            return View(files);
        }

        [HttpGet]
        public IActionResult EditFileData(int Id , int Status)
        {
            //var result =
            //    await _dataClientHelper.CallAPI<List<DataFiles>, List<DataFiles>>("/api/DataEgress/UpdateFileData", new List<DataFiles>() { model });
            //var result = _dataClientHelper.CallAPIWithoutModel<DataFiles>("/api/DataEgress/UpdateFileData",
            //      new Dictionary<string, string>()
            //          { { "Id", Id.ToString() }, { "Status", Status.ToString() } })
            //  .Result;
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("id", Id.ToString());
            paramlist.Add("Status", Status.ToString());
            var files = _dataClientHelper.CallAPIWithoutModel<DataFiles>("/api/DataEgress/UpdateFileData/", paramlist).Result;

            return RedirectToAction("GetFiles", new { Id = Id });
        }
    
        
        [HttpGet]
        public IActionResult DownloadFile(int? FileId)
        {

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("FileId", FileId.ToString());
            var file = _dataClientHelper.CallAPIWithoutModel<DataFiles>(
                "/api/DataEgress/DownloadFile", paramlist).Result;
            return View(file);
        }

        [HttpGet]
        public IActionResult GetAllFiles()
        {
            var files = _dataClientHelper.CallAPIWithoutModel<List<DataFiles>>("/api/DataEgress/GetAllFiles/").Result;

            var filecount = 0;
            var submissionIDCounts = files
            .GroupBy(file => file.SubmissionId)
            .Select(group => new { submissionId = group.Key, Count = group.Count() })
            .ToList();
            foreach (var count in submissionIDCounts)
            {
                filecount = count.Count;
            }

            ViewBag.Filecount = filecount;
            return View(files);
        }



    }
}