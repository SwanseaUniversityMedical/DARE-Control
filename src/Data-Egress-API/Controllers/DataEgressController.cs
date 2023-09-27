using BL.Models;
using BL.Models.APISimpleTypeReturns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data_Egress_API.Repositories.DbContexts;
using Castle.Components.DictionaryAdapter.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using Serilog;

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

        [HttpPost(Name = "AddNewDataEgress")]
        public async Task<BoolReturn> AddNewDataEgress(int submissionId, List<string> files)
        {
            var existingSubmission = _DbContext.DataEgressFile
                .Include(d => d.files)
               .FirstOrDefault(d => d.Id == submissionId);

            if (existingSubmission != null)
            {
                foreach (var file in files)
                {
                    var dataEgressFile = new DataEgressFiles()
                    {
                        submissionId = submissionId,
                        FileStatus = "",
                        Reviewer = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                        FileSize = "",
                        FileName = "",
                        LastUpdate = DateTime.Now.ToUniversalTime()

                    };

                    _DbContext.DataEgressFile.Add(dataEgressFile);
                  
                }
                await _DbContext.SaveChangesAsync();
                return new BoolReturn() { Result = true };
            }
            else
            {
                return new BoolReturn() { Result = false };
            }
       
        }
       
        [HttpGet(Name = "GetAllFiles")]
        public List<DataEgressFiles> GetAllFiles()
        {
            try
            {
                var allFiles = _DbContext.DataEgressFile.ToList();

                Log.Information("{Function} Files retrieved successfully", "GetAllFiles");
                return allFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllFiles");
                throw;
            }
        }
        [HttpGet(Name = "GetAllUnprocessedFiles")]
        public List<DataEgressFiles> GetAllUnprocessedFiles()
        {      
            try
            {
                var allUnprocessedFiles = _DbContext.DataEgressFile.Where(x =>x.FileStatus !="Approved").ToList();
  
                Log.Information("{Function} Files retrieved successfully", "GetAllUnprocessedFiles");
                return allUnprocessedFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "allUnprocessedFiles");
                throw;
            }
        }

    }

}