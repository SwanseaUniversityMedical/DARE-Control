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
        public async Task<BoolReturn> AddNewDataEgress(int submissionId, List<IFormFile> files)
        {
            var existingSubmission = _DbContext.DataEgressFile
                .Include(d => d.files)
               .FirstOrDefault(d => d.Id == submissionId);

            if (existingSubmission != null)
            {
                foreach (var file in files)
                {
                    var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Files\\");
                    bool basePathExists = System.IO.Directory.Exists(basePath);
                    if (!basePathExists) Directory.CreateDirectory(basePath);
                    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var filePath = Path.Combine(basePath, file.FileName);
                    var extension = Path.GetExtension(file.FileName);

                    var dataEgressFile = new DataEgressFiles()
                    {

                        submissionId = submissionId,
                        Reviewer = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                        FileSize = file.Length.ToString(),
                        FileName = fileName,
                        FileType = extension,
                        LastUpdate = DateTime.Now.ToUniversalTime()

                    };
                    using (var dataStream = new MemoryStream())
                    {
                        await file.CopyToAsync(dataStream);
                        dataEgressFile.FileData = dataStream.ToArray();
                    }

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

        [HttpGet("GetAllFiles")]
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

        [HttpGet("GetAllUnprocessedFiles")]
        public List<DataEgressFiles> GetAllUnprocessedFiles()
        {
            try
            {
                var allUnprocessedFiles = _DbContext.DataEgressFile.Where(x => x.FileStatus != "Approved").ToList();

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