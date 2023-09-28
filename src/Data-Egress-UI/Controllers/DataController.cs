﻿using Data_Egress_UI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BL.Services;
using BL.Models;

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

            var files = _dataClientHelper.CallAPIWithoutModel<List<DataEgressFiles>>("/api/DataEgress/GetAllFiles/").Result;
            return View(files);


        }

        [HttpGet]
        public IActionResult GetAllUnprocessedFiles()
        {

            var unprocessedfiles = _dataClientHelper.CallAPIWithoutModel<List<DataEgressFiles>>("/api/DataEgress/GetAllUnprocessedFiles/").Result;
            return View(unprocessedfiles);


        }
       
    }
}