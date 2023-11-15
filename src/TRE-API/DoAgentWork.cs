using Microsoft.Extensions.Configuration;
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Models.Tes;
using BL.Rabbit;
using Microsoft.Extensions.DependencyInjection;
using EasyNetQ;
using Newtonsoft.Json;
using Serilog;

using Microsoft.Extensions.Hosting;
using Hangfire;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;

using System.Threading.Tasks;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;
using BL.Services;
using Microsoft.AspNetCore.SignalR;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.Extensions.Configuration;
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Models.Tes;
using BL.Rabbit;
using Microsoft.Extensions.DependencyInjection;
using EasyNetQ;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.Hosting;
using Hangfire;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;
using BL.Services;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using TREAgent.Repositories;
using System.Net.Http.Json;
using TRE_API.Models;
using Castle.Components.DictionaryAdapter.Xml;
using System;
using System.Net;

namespace TRE_API
{
    public interface IDoAgentWork
    {
        void Execute();
        Task CheckTESK(string taskID, int subId, string tesId, string outputBucket);
        void ClearJob(string jobname);
        Task testing(string toRun, string Role);
    }

    // TESK : http://172.16.34.31:8080/    https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks


    public class DoAgentWork : IDoAgentWork
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISubmissionHelper _subHelper;
        private readonly IMinioSubHelper _minioSubHelper;
        private readonly IMinioTreHelper _minioTreHelper;
        private readonly IHasuraAuthenticationService _hasuraAuthenticationService;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly AgentSettings _AgentSettings;
        private readonly MinioSettings _minioSettings;


        public DoAgentWork(IServiceProvider serviceProvider,
            ApplicationDbContext dbContext,
            ISubmissionHelper subHelper, 
            IMinioTreHelper minioTreHelper,
            IMinioSubHelper minioSubHelper,
            IHasuraAuthenticationService hasuraAuthenticationService,
            IDareClientWithoutTokenHelper dareHelper,
            AgentSettings AgentSettings,
            MinioSettings minioSettings)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _subHelper = subHelper;

            _minioTreHelper = minioTreHelper;
            _minioSubHelper = minioSubHelper;

            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _subHelper = subHelper;

            _hasuraAuthenticationService = hasuraAuthenticationService;
            _dareHelper = dareHelper;
            _AgentSettings = AgentSettings;
            _minioSettings = minioSettings;

            _minioTreHelper = minioTreHelper;
            _minioSubHelper = minioSubHelper;
        }

        public async Task testing(string toRun, string Role)
        {


            //TesId

            Console.WriteLine("Testing");
            Log.Information("{Function}  SEND TO TESK ", "Execute");
            var arr = new HttpClient();



            var role = Role; 

            var Token = _hasuraAuthenticationService.GetNewToken(role);



            var projectId = 1;



            var OutputBucket = _AgentSettings.TESKOutputBucketPrefix + _dbContext.Projects.First(x => x.Id == projectId).OutputBucketTre; //TODO Check, Projects not getting The synchronised Properly 
            var tesMessage = JsonConvert.DeserializeObject<TesTask>(toRun);                                                                                                                 //it need the file name?? (key-name)

            if (tesMessage.Outputs == null)
            {
                tesMessage.Outputs = new List<TesOutput> { };
            }

            //S3://bucket-name/key-name
            foreach (var output in tesMessage.Outputs)
            {
                output.Url = OutputBucket;
            }

            if (tesMessage.Executors == null)
            {
                tesMessage.Executors = new List<TesExecutor>();
            }

            foreach (var Executor in tesMessage.Executors)
            {
                if (Executor.Image == "CustomerImages") //TODO
                {
                    for (int i = 0; i < Executor.Command.Count; i++)
                    {
                        Executor.Command[i] += "--" + Token;
                    }
                }

                //if (Executor.Env == null)
                //{
                //    Executor.Env = new Dictionary<string, string>();
                //}
                //Executor.Env["HASURAAuthenticationToken"] = Token;
            }

            _dbContext.TokensToExpire.Add(new TokenToExpire()
            {
                SubId = 99,
                Token = Token
            });
            _dbContext.SaveChanges();



            if (tesMessage is not null)
            {
                var stringdata = JsonConvert.SerializeObject(tesMessage);
                Log.Information("{Function} tesMessage is not null runhing CreateTESK {tesMessage}", "Execute", stringdata);
                CreateTESK(stringdata, 99, "123COOOLLL", OutputBucket);
            }

 
        }

        public string CreateTESK(string jsonContent, int subId, string tesId, string outputBucket)
        {

            Log.Information("{Function} {jsonContent} runhing CreateTESK ", "CreateTESK", jsonContent);

            HttpClientHandler handler = new HttpClientHandler();

            if (_AgentSettings.Proxy)
            {
                handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(_AgentSettings.ProxyAddresURL, true), // Replace with your proxy server URL
                    UseProxy = _AgentSettings.Proxy,
                };
            }


            using (var httpClient = new HttpClient(handler))
            {
                // Define the URL for the POST request
                string apiUrl = "https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks";

                // Create a HttpRequestMessage with the HTTP method set to POST
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Set the headers
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                // Attach the JSON string to the request's content
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


                // Send the POST request
                HttpResponseMessage response = httpClient.SendAsync(request).Result;

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    Log.Information("{Function} Request successful. Response: {Response}", "CreateTESK", responseBody);
                    Console.WriteLine("Request successful. Response:");
                    Console.WriteLine(responseBody);
                    Log.Information("Request successful. Response: {response}", responseBody);


                    var responseObj = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                    string id = responseObj.id;

                    RecurringJob.AddOrUpdate<IDoAgentWork>(id, a => a.CheckTESK(id, subId, tesId, outputBucket),
                        Cron.MinuteInterval(1));

                    _dbContext.Add(new TeskAudit() { message = jsonContent, teskid = tesId, subid = subId.ToString() });
                    _dbContext.SaveChanges();

                    return id;
                }
                else
                {
                    Log.Error("{Function} Request failed with status code: {Code}", "CreateTESK", response.StatusCode);
                    
                    return "";
                }
            }
        }

        class ResponseModel
        {
            public string id { get; set; }
        }

        public async Task CheckTESK(string taskID, int subId, string tesId, string outputBucket)
        {
            
            Log.Information("{Function} Check TESK : {TaskId},  TES : {TesId}, sub: {SubId}", "CheckTESK", taskID, tesId, subId);
            string url = "https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks/" + taskID + "?view=basic";

            HttpClientHandler handler = new HttpClientHandler();

            if (_AgentSettings.Proxy)
            {
                handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(_AgentSettings.ProxyAddresURL, true), // Replace with your proxy server URL
                    UseProxy = _AgentSettings.Proxy,
                };
            }

            using (HttpClient client = new HttpClient(handler))
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                Log.Information("{Function} Response status {State}", "CheckTESK", response.StatusCode);
                Console.WriteLine(response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;


                    TESKstatus status = JsonConvert.DeserializeObject<TESKstatus>(content);

                    var shouldReport = false;

                    var fromDatabase = (_dbContext.TESK_Status.Where(x => x.id == status.id)).FirstOrDefault();

                    if (fromDatabase is null)
                    {
                        shouldReport = true;
                        _dbContext.Add(status);
                    }
                    else
                    {
                        if (fromDatabase.state != status.state)
                        {
                            shouldReport = true;
                            fromDatabase.state = status.state;
                            _dbContext.Update(fromDatabase);

                        }
                    }

                    _dbContext.SaveChanges();

                    if (shouldReport == true || (status.state == "COMPLETE" || status.state == "EXECUTOR_ERROR"))
                    {
                        Log.Information("{Function} *** status change *** {State}", "CheckTESK", status.state);
                        

                        // send update
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            TokenToExpire Token = null;
                            var statusMessage = StatusType.TransferredToPod;
                            switch (status.state)
                            {
                                case "QUEUED":
                                    statusMessage = StatusType.TransferredToPod;
                                    break;
                                case "RUNNING":
                                    statusMessage = StatusType.PodProcessing;
                                    break;
                                case "COMPLETE":
                                    statusMessage = StatusType.PodProcessingComplete;
                                    
                                    Token = _dbContext.TokensToExpire.FirstOrDefault(x => x.SubId == subId);
                                    Log.Information("{Function} *** COMPLETE remove Token *** {Token} ", "CheckTESK", Token);
                                    
                                    if (Token != null)
                                    {
                                        _dbContext.TokensToExpire.Remove(Token);
                                        _hasuraAuthenticationService.ExpirerToken(Token.Token);
                                    }
                                    
                                    _dbContext.SaveChanges();
                                    break;
                                case "EXECUTOR_ERROR":
                                    statusMessage = StatusType.Cancelled;
                                    Token = _dbContext.TokensToExpire.FirstOrDefault(x => x.SubId == subId);
                                    Log.Information("{Function} *** EXECUTOR_ERROR remove Token *** {Token} ", "CheckTESK", Token);
                                    
                                    if (Token != null)
                                    {
                                        _dbContext.TokensToExpire.Remove(Token);
                                        _hasuraAuthenticationService.ExpirerToken(Token.Token);
                                    }
                                    
                                    _dbContext.SaveChanges();

                                    break;
                            }


                            var result = _subHelper.UpdateStatusForTre(subId.ToString(), statusMessage, "");
                            if (status.state == "COMPLETE")
                            {
                                Log.Information($"  CloseSubmissionForTre with status.state subId {subId.ToString()} == COMPLETE ");
                                result = _subHelper.CloseSubmissionForTre(subId.ToString(), StatusType.Completed, "","");
                            }
                            else if (status.state == "EXECUTER_ERROR")
                            {
                                Log.Information($"  CloseSubmissionForTre with status.state subId {subId.ToString()} == EXECUTER_ERROR ");
                                result = _subHelper.CloseSubmissionForTre(subId.ToString(), StatusType.Failed, "", "");
                            }
                        }
                        Log.Information($" Checking status ");
                        // are we done ?
                        if (status.state == "COMPLETE" || status.state == "EXECUTOR_ERROR")
                        {
                            Log.Information($"  status.state == \"COMPLETE\" || status.state == \"EXECUTOR_ERROR\" ");

                            ClearJob(taskID);
                            var outputBucketGood = outputBucket.Replace(_AgentSettings.TESKOutputBucketPrefix, "");
                            var data = await _minioTreHelper.GetFilesInBucket(outputBucketGood);
                            var files = new List<string>();

                            foreach (var s3Object in data.S3Objects) //TODO is this right?
                            {
                                Log.Information("{Function} *** added file from outputBucket *** {file} ", "CheckTESK", s3Object.Key);
                                files.Add(s3Object.Key);
                            }

                            Log.Information($"  FilesReadyForReview files {files.Count} ");
                            _subHelper.FilesReadyForReview(new ReviewFiles()
                            {
                                SubId = taskID, //TODO is this right  
                                Files = files
                            }, outputBucketGood);

                        }
                    }
                    else
                        Log.Information("{Function} No change", "CheckTESK");
                    


                }
                else
                {
                    Log.Error("{Function} HTTP Request {url} failed with status code {code}", "CheckTESK", url, response.StatusCode);
                    
                }
            }


        }

        //TODO Implement proper check
        private bool ValidateCreate(Submission sub)
        {
            return true;
        }

        // Method executed upon hangfire job
        public void Execute()
        {
            Log.Information("{Function} DoAgentWork ruinng", "Execute");
            // control use of dependency injection
            using (var scope = _serviceProvider.CreateScope())
            {
                // OPTIONS
                var useRabbit = _AgentSettings.UseRabbit;
                var useHutch = _AgentSettings.UseHutch;
                var useTESK = _AgentSettings.UseTESK;

                Log.Information("{Function} useRabbit {useRabbit}", "Execute", useRabbit);
                Log.Information("{Function} useHutch {useHutch}", "Execute", useHutch);
                Log.Information("{Function} useTESK {useTESK}", "Execute", useTESK);

                var cancelsubprojs = _subHelper.GetRequestCancelSubsForTre();
                if (cancelsubprojs != null)
                {
                    foreach (var cancelsubproj in cancelsubprojs)
                    {
                        _subHelper.UpdateStatusForTre(cancelsubproj.Id.ToString(), StatusType.CancellationRequestSent,
                            "");
                        //TODO Do we need to call Hutch or other stuff to cancel and do other cancel stuff
                        _subHelper.CloseSubmissionForTre(cancelsubproj.Id.ToString(), StatusType.Cancelled, "", "");
                    }

                }

                // Get list of submissions
                List<Submission> listOfSubmissions;
                
                try
                {
                    listOfSubmissions = _subHelper.GetWaitingSubmissionForTre();
                }
                catch (Exception e)
                {
                    Log.Error(e,"{Function} Error getting submissions", "Execute");
                    
                    throw;
                }


                Log.Information("{Function} listOfSubmissions {listOfSubmissions}", "Execute", listOfSubmissions?.Count);
                foreach (var aSubmission in listOfSubmissions)
                {
                    try
                    {
                        Log.Information("{Function}Submission: {submission}", "Execute", aSubmission.Id);

                        // Check user is allowed ont he project
                        if (!_subHelper.IsUserApprovedOnProject(aSubmission.Project.Id, aSubmission.SubmittedBy.Id))
                        {
                            Log.Error("{Function }User {UserID}/project {ProjectId} is not value for this submission {submission}","Execute",
                                aSubmission.SubmittedBy.Id, aSubmission.Project.Id, aSubmission);
                            // record error with submission layer
                            var result =
                                _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.InvalidUser, "");
                            result = _subHelper.CloseSubmissionForTre(aSubmission.Id.ToString(), StatusType.Failed, "",
                                "");
                        }
                        else
                        {


                            try
                            {
                                if (useTESK == false)
                                {
                                    Uri uri = new Uri(aSubmission.DockerInputLocation);
                                    string fileName = Path.GetFileName(uri.LocalPath);
                                    var sourceBucket = aSubmission.Project.SubmissionBucket;
                                    var subProj = _dbContext.Projects
                                        .FirstOrDefault(x => x.SubmissionProjectId == aSubmission.Project.Id);

                                    var destinationBucket = subProj.SubmissionBucketTre;
                                    var source = _minioSubHelper.GetCopyObject(sourceBucket, fileName);
                                    var resultcopy = _minioTreHelper
                                        .CopyObjectToDestination(destinationBucket, fileName, source.Result).Result;
                                }

                                _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(),
                                    StatusType.TreWaitingForCrateFormatCheck, "");
                                if (ValidateCreate(aSubmission))
                                {
                                    _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.TreCrateValidated,
                                        "");
                                }
                                else
                                {
                                    _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(),
                                        StatusType.SubmissionCrateValidationFailed, "");
                                    _subHelper.CloseSubmissionForTre(aSubmission.Id.ToString(), StatusType.Failed, "", "");
                                }

                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                                throw;
                            }



                            // The TES message
                            var tesMessage = JsonConvert.DeserializeObject<TesTask>(aSubmission.TesJson);
                            var processedOK = true;

                            // **************  SEND TO RABBIT
                            if (useRabbit)
                            {
                                try
                                {
                                    // Not ideal to create each time around the loop but ???
                                    IBus rabbit = scope.ServiceProvider.GetRequiredService<IBus>();
                                    EasyNetQ.Topology.Exchange exchangeObject =
                                        rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Submission, "topic");
                                    rabbit.Advanced.Publish(exchangeObject, RoutingConstants.ProcessSub, false,
                                        new Message<TesTask>(tesMessage));
                                }
                                catch (Exception e)
                                {
                                    
                                    Log.Error(e, "{Function} Send rabbit failed for sub {SubId}", "Execute", aSubmission.Id);

                                    processedOK = false;
                                }
                            }

                            // **************  SEND TO HUTCH
                            if (useHutch)
                            {
                                // TODO for rest API
                                try
                                {

                                    _subHelper.SendSumissionToHUTCH(aSubmission);

                                }
                                catch (Exception e)
                                {
                                    Log.Error(e, "{Function} Send HUTCH failed for sub {SubId}", "Execute", aSubmission.Id);
                                    processedOK = false;
                                }
                            }

                            // **************  SEND TO TESK
                            if (useTESK)
                            {

                                Log.Information("{Function}  SEND TO TESK ", "Execute");
                                var arr = new HttpClient();



                                var role = aSubmission.Project.Name; //TODO Check

                                var Token = _hasuraAuthenticationService.GetNewToken(role);



                                var projectId = aSubmission.Project.Id;



                                var OutputBucket = _AgentSettings.TESKOutputBucketPrefix + _dbContext.Projects.First(x => x.Id == projectId).OutputBucketTre; //TODO Check, Projects not getting The synchronised Properly 
                                                                                                                                                              //it need the file name?? (key-name)

                                if (tesMessage.Outputs == null)
                                {
                                    tesMessage.Outputs = new List<TesOutput> {};
                                }

                                //S3://bucket-name/key-name
                                foreach (var output in tesMessage.Outputs)
                                {
                                    output.Url = OutputBucket;
                                }

                                if (tesMessage.Executors == null)
                                {
                                    tesMessage.Executors = new List<TesExecutor>();
                                }
                                Log.Information("looking for _AgentSettings.ImageNameToAddToToken > " + _AgentSettings.ImageNameToAddToToken);
                                foreach (var Executor in tesMessage.Executors)
                                {
                                    Log.Information("Executor.Image > " + Executor.Image);

                                    if (Executor.Image.Contains(_AgentSettings.ImageNameToAddToToken))
                                    {
                                        Executor.Command.Add("--Token_" + Token);
									}

                                    //if (Executor.Env == null)
                                    //{
                                    //    Executor.Env = new Dictionary<string, string>();
                                    //}
                                    //Executor.Env["HASURAAuthenticationToken"] = Token;
                                }

                                _dbContext.TokensToExpire.Add(new TokenToExpire()
                                {
                                    SubId = aSubmission.Id,
                                    Token = Token
                                });
                                _dbContext.SaveChanges();



                                if (tesMessage is not null)
                                {
                                    var stringdata = JsonConvert.SerializeObject(tesMessage);
                                    Log.Information("{Function} tesMessage is not null runhing CreateTESK {tesMessage}", "Execute", stringdata);
                                    CreateTESK(stringdata, aSubmission.Id, aSubmission.TesId, OutputBucket);
                                }

                            }

                            // **************  TELL SUBMISSION LAYER WE DONE
                            if (processedOK)
                            {
                                try
                                {
                                    var result = _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(),
                                        StatusType.TransferredToPod, "");
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e,"{Function} Error sending record outcome to submission layer for sub {SubId}", "Execute", aSubmission.Id);
                                    processedOK = false;
                                }

                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "{Function } Error occured processing submission {SubId}", "Execute", aSubmission.Id);
                        
                    }
                }
            }

        }

        public void ClearJob(string jobname)
        {
            Log.Information("{Function} Hangfire clear job: {Jobname}", "ClearJob", jobname);
            
            RecurringJob.RemoveIfExists(jobname);
        }
    }

}