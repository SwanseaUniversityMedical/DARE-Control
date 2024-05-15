using BL.Models;
using BL.Models.APISimpleTypeReturns;
using Microsoft.AspNetCore.Mvc;
using Data_Egress_API.Repositories.DbContexts;
using Castle.Components.DictionaryAdapter.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using BL.Models.Enums;
using EasyNetQ.Management.Client.Model;
using System.IO;
using BL.Models.ViewModels;
using Amazon.S3.Model;
using Amazon.S3;
using Amazon;
using System.Net;
using System.Collections.Generic;
using BL.Services;
using System.Net.Mime;
using Data_Egress_API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Sentry.PlatformAbstractions;
using DARE_Egress.Services;

namespace Data_Egress_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class DataEgressController : ControllerBase
    {
        private readonly ApplicationDbContext _DbContext;

        private readonly ITreClientWithoutTokenHelper _treClientHelper;
        private readonly IMinioHelper _minioHelper;
        private readonly IDareEmailService _IDareEmailService;
        private readonly IKeyCloakService _IKeyCloakService;
        private readonly EmailSettings _EmailSettings;


        public DataEgressController(ApplicationDbContext repository, ITreClientWithoutTokenHelper treClientHelper,
            IMinioHelper minioHelper, IDareEmailService iDareEmailService, EmailSettings emailSettings)
        {
            _DbContext = repository;
            _minioHelper = minioHelper;
            _treClientHelper = treClientHelper;
            _IDareEmailService = iDareEmailService;
            _EmailSettings = emailSettings;
        }

        [Authorize(Roles = "data-egress-admin,dare-tre-admin")]
        [HttpPost("AddNewDataEgress")]
        public async Task<BoolReturn> AddNewDataEgress(EgressSubmission submission)
        {
            try
            {
                var existingDataFiles = _DbContext.EgressSubmissions
                    .FirstOrDefault(d => d.SubmissionId == submission.SubmissionId);


                if (existingDataFiles == null)
                {
                    _DbContext.EgressSubmissions.Add(submission);
                    foreach (var submissionFile in submission.Files)
                    {
                        submissionFile.Status = FileStatus.Undecided;
                    }

                    await _DbContext.SaveChangesAsync();
                    try
                    {
                        var Emails = await _IKeyCloakService.GetEmailsOfAccountWithRole("data-egress-admin");

                        if (string.IsNullOrEmpty(_EmailSettings.EmailOverride) == false)
                        {
                            Emails = new List<string>() { _EmailSettings.EmailOverride };
                        }

                        foreach (var email in Emails)
                        {
                            var body = $" files are ready to review with subID {submission.Id} Name {submission.Name}";
                            await _IDareEmailService.EmailTo(email, body, body, false);
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }                  
                    
                    return new BoolReturn() { Result = true };
                }
                else
                {
                    return new BoolReturn() { Result = false };
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddNewDataEgress");
                throw;
            }
        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("GetAllEgresses")]
        public List<EgressSubmission> GetAllEgresses(bool unprocessedonly)
        {
            try
            {
                if (unprocessedonly)
                {
                    var results = _DbContext.EgressSubmissions.ToList();
                    Log.Information("{Function} All Egresses retrieved successfully", "GetAllEgresses");
                    return results;
                }
                else
                {
                    var allUnprocessedFiles = _DbContext.EgressSubmissions.Where(x => x.Status == EgressStatus.NotCompleted)
                        .ToList();

                    Log.Information("{Function} All Unprocessed Egresses retrieved successfully", "GetAllEgresses");
                    return allUnprocessedFiles;
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllEgresses");
                throw;
            }
        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("GetEgress")]
        public EgressSubmission GetEgress(int id)
        {
            try
            {
                var returned = _DbContext.EgressSubmissions.First(x => x.Id == id);


                Log.Information("{Function} Files retrieved successfully", "GetEgress");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetEgress");
                throw;
            }


        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("GetEgressFile/{id}")]
        public EgressFile GetEgressFile(int id)
        {
            try
            {
                var returned = _DbContext.EgressFiles.First(x => x.Id == id);


                Log.Information("{Function} File retrieved successfully", "GetEgressFile");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetEgressFile");
                throw;
            }


        }

        

        [Authorize(Roles = "data-egress-admin")]
        [HttpPost("CompleteEgress")]
        public async Task<EgressSubmission> CompleteEgressAsync([FromBody] EgressSubmission egress)
        {
            try
            {
                var dbegress = _DbContext.EgressSubmissions.First(x => x.Id == egress.Id);
                if (dbegress.Status != EgressStatus.NotCompleted)
                {
                    throw new Exception("Egress has already been completed");
                }

                var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(approvedBy))
                {
                    approvedBy = "[Unknown]";
                }

                var approvedDate = DateTime.Now.ToUniversalTime();
                if (egress.Files.Any(x => x.Status == FileStatus.Undecided))
                {
                    throw new Exception("Not all files reviewed");
                }
                else if (egress.Files.All(x => x.Status == FileStatus.Rejected))
                {
                    dbegress.Status = EgressStatus.FullyRejected;
                }
                else if (egress.Files.All(x => x.Status == FileStatus.Approved))
                {
                    dbegress.Status = EgressStatus.FullyApproved;
                }
                else
                {
                    dbegress.Status = EgressStatus.PartiallyApproved;
                }

                var backtotre = ReviewFilesGuts(egress, dbegress, approvedBy, approvedDate);

                var result =
                    await _treClientHelper.CallAPI<EgressReview, Submission>("/api/Submission/EgressResults",
                        backtotre);
                await _DbContext.SaveChangesAsync();









                Log.Information("{Function} Egress Completed for Submission {SubId}", "CompleteEgress",
                    dbegress.SubmissionId);
                return dbegress;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "CompleteEgress");
                throw;
            }

        }

        private static EgressReview ReviewFilesGuts(EgressSubmission egress, EgressSubmission dbegress,
            string approvedBy,
            DateTime approvedDate)
        {
            dbegress.Reviewer = approvedBy;
            dbegress.Completed = approvedDate;

            var backtotre = new EgressReview()
            {
                SubId = dbegress.SubmissionId,
                FileResults = new List<EgressResult>(),
                OutputBucket = dbegress.OutputBucket
            };
            foreach (var file in egress.Files)
            {
                var dbegressfile = dbegress.Files.First(x => x.Id == file.Id);
                dbegressfile.Status = file.Status;
                backtotre.FileResults.Add(new EgressResult()
                {
                    FileName = dbegressfile.Name,
                    Approved = dbegressfile.Status == FileStatus.Approved,
                });
                var egfile = dbegress.Files.First(x => x.Id == file.Id);
                egfile.Status = file.Status;
                egfile.LastUpdate = approvedDate;
                egfile.Reviewer = approvedBy;
            }

            return backtotre;
        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpPost("PartialEgress")]
        public async Task<EgressSubmission> PartialEgressAsync([FromBody] EgressSubmission egress)
        {

            var dbegress = _DbContext.EgressSubmissions.First(x => x.Id == egress.Id);
            if (dbegress.Status != EgressStatus.NotCompleted)
            {
                throw new Exception("Egress has already been completed");
            }

            var approvedBy =
                (from x in User.Claims where x.Type == "preferred_username" select x.Value).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(approvedBy))
            {
                approvedBy = "[Unknown]";
            }

            var approvedDate = DateTime.Now.ToUniversalTime();

            var backtotre = ReviewFilesGuts(egress, dbegress, approvedBy, approvedDate);


            await _DbContext.SaveChangesAsync();









            Log.Information("{Function} Egress Completed for Submission {SubId}", "CompleteEgress",
                dbegress.SubmissionId);
            return dbegress;
        }



        [Authorize(Roles = "data-egress-admin")]
        [HttpPost("UpdateFileData")]
        public EgressFile UpdateFileData(int fileId, FileStatus status)
        {
            try
            {
                var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(approvedBy))
                {
                    approvedBy = "[Unknown]";
                }

                var approvedDate = DateTime.Now.ToUniversalTime();

                var returned = _DbContext.EgressFiles.First(x => x.Id == fileId);

                returned.Status = status;
                returned.Reviewer = approvedBy;
                returned.LastUpdate = approvedDate;


                _DbContext.SaveChangesAsync();
                Log.Information("{Function} File updated", "UpdateFileData");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "UpdateFileData");
                throw;
            }

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

        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFileAsync(int id)
        {
            try
            {

                var egressFile = _DbContext.EgressFiles.First(x => x.Id == id);

                var response = await _minioHelper.GetCopyObject(egressFile.EgressSubmission.OutputBucket, egressFile.Name);

                var responseStream = response.ResponseStream;

                return File(responseStream, GetContentType(egressFile.Name), egressFile.Name);
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "DownloadFiles");
                throw;
            }

        }





    }

}

