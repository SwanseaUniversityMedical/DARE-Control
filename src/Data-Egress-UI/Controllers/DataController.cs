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
using Amazon.S3.Model;
using BL.Models.ViewModels;

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

        //[HttpGet]
        //public IActionResult DownloadFile(int? FileId)
        //{
          
        //    var paramlist = new Dictionary<string, string>();
        //    paramlist.Add("FileId", FileId.ToString());
        //    var file = _dataClientHelper.CallAPIWithoutModel<DataFiles>(
        //        "/api/DataEgress//", paramlist).Result;
        //    return View(file);
        //}
        //public async IActionResult DownloadFile(MinioSettings minioSettings, string bucketName = "", string objectName = "")
        //{
        //    var request = new GetObjectRequest
        //    {
        //        BucketName = bucketName,
        //        Key = objectName,
        //    };

        //    var amazonS3Client = GenerateAmazonS3Client(minioSettings);

        //    var objectExists = await CheckObjectExists(_minioSettings, request.BucketName, request.Key);

        //    if (objectExists)
        //    {
        //        var response = await amazonS3Client.GetObjectAsync(request);

        //        using (var responseStream = response.ResponseStream)
        //        {
        //            string saveFilePath = "C:/Path/To/Save/File.txt";

        //            using (var fileStream = new FileStream(saveFilePath, FileMode.Create))
        //            {
        //                await responseStream.CopyToAsync(fileStream);
        //            }
        //        }

        //        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            Log.Warning($"Successfully uploaded {objectName} to {bucketName}.");
        //            return true;
        //        }
        //        else
        //        {
        //            Log.Warning($"Could not upload {objectName} to {bucketName}.");
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }


        //}
    }
}