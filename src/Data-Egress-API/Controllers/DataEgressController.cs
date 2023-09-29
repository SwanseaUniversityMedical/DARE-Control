using BL.Models;
using BL.Models.APISimpleTypeReturns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data_Egress_API.Repositories.DbContexts;
using Castle.Components.DictionaryAdapter.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using BL.Models.Enums;
using EasyNetQ.Management.Client.Model;

namespace Data_Egress_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataEgressController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;

        public DataEgressController(ApplicationDbContext repository)
        {
            _DbContext = repository;

        }
        [HttpPost("AddNewDataEgress")]
        public async Task<BoolReturn> AddNewDataEgress(int submissionId, List<SubmissionFile> files)
        {
            var existingDataFiles = _DbContext.DataEgressFiles            
                .FirstOrDefault(d => d.Id == submissionId);

            var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            if (existingDataFiles != null)
            {
                foreach (var file in files)
                {
                   
                    var dataFile = new DataFiles()
                    {
                        Name = file.Name,
                        TreBucketFullPath = file.TreBucketFullPath,
                        SubmisionBucketFullPath = file.SubmisionBucketFullPath,
                        Status = file.Status,
                        Description = file.Description,
                        LastUpdate = DateTime.Now.ToUniversalTime(),
                        Reviewer = approvedBy,
                        SubmissionId = file.Id
                    };
                    _DbContext.DataEgressFiles.Add(dataFile);
                
                }
                await _DbContext.SaveChangesAsync();
                return new BoolReturn() { Result = true };
            }
            else
            {
                return new BoolReturn() { Result = false };
            }

        }


        [HttpGet("GetAllFiles")]
        public List<DataFiles> GetAllFiles()
        {
            try
            {
                var allFiles = _DbContext.DataEgressFiles.ToList();

                Log.Information("{Function} Files retrieved successfully", "GetAllFiles");
                return allFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllFiles");
                throw;
            }
        }

        //[HttpGet("GetAllUnprocessedFiles")]
        //public List<DataEgressFiles> GetAllUnprocessedFiles()
        //{
        //    try
        //    {
        //        var allUnprocessedFiles = _DbContext.DataEgressFile.Where(x => x.FileStatus != "Approved").ToList();

        //        Log.Information("{Function} Files retrieved successfully", "GetAllUnprocessedFiles");
        //        return allUnprocessedFiles;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "{Function} Crashed", "allUnprocessedFiles");
        //        throw;
        //    }
        //}



    }

}