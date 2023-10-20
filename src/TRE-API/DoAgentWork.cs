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

namespace TRE_API
{
    public interface IDoAgentWork
    {
        void Execute(bool useRabbit = true, bool useHutch = false, bool useTESK = true);
        void CheckTESK(string taskID, string TesId);
        void ClearJob(string jobname);
        void testing();
    }

    // TESK : http://172.16.34.31:8080/    https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks


    public class DoAgentWork : IDoAgentWork
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISubmissionHelper _subHelper;
        
        private readonly IMinioSubHelper _minioSubHelper;
        private readonly IMinioTreHelper _minioTreHelper;
        public DoAgentWork(IServiceProvider serviceProvider, ApplicationDbContext dbContext, ISubmissionHelper subHelper,  IMinioTreHelper minioTreHelper, IMinioSubHelper minioSubHelper)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _subHelper = subHelper;
            
            _minioTreHelper = minioTreHelper;
            _minioSubHelper = minioSubHelper;
        }

        public void testing()
        {

            Console.WriteLine("Testing");
            string jsonContent = "{ \"name\": \"Hello World\", \"description\": \"Hello World, inspired by Funnel's most basic example\",\r\n\"executors\": [\r\n{ \"image\": \"alpine\", \"command\": [ \"sleep\", \"5m\" ] },\r\n{ \"image\": \"alpine\", \"command\": [ \"echo\", \"TESK says:   Hello World\" ]    }\r\n  ]\r\n}";

            CreateTESK(jsonContent,"99");
        }

        public string CreateTESK(string jsonContent, string TesId)
        {
            using (var httpClient = new HttpClient())
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
                    Console.WriteLine("Request successful. Response:");
                    Console.WriteLine(responseBody);
                    Log.Information("Request successful. Response: {response}", responseBody);

                    var responseObj = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                    string id = responseObj.id;

                    RecurringJob.AddOrUpdate<IDoAgentWork>(id, a => a.CheckTESK(id,TesId), Cron.MinuteInterval(1));

                    _dbContext.Add(new TeskAudit(){message = jsonContent, teskid = id});
                    _dbContext.SaveChanges();

                    return id;
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    return "";
                }
            }
        }

        class ResponseModel
        {
            public string id { get; set; }
        }
        public void CheckTESK(string taskID, string TesId)
        {
            Console.WriteLine("Check TESK : "+taskID + ",  TES : "+TesId);

            string url = "https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks/"+taskID+"?view=basic";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response =  client.GetAsync(url).Result;

                Console.WriteLine(response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
//                    Console.WriteLine(content);

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
                            Console.WriteLine("*** status change *** " + status.state);

                            // send update
                            using (var scope = _serviceProvider.CreateScope())
                            {
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
                                        break;
                                    case "EXECUTOR_ERROR":
                                        statusMessage = StatusType.Cancelled;
                                        break;
                                }


                                var result = _subHelper.UpdateStatusForTre(TesId, statusMessage, "");
                                if (status.state == "COMPLETE")
                                {
                                    result = _subHelper.CloseSubmissionForTre(TesId.ToString(), StatusType.Completed, "", "");
                            }
                                else if (status.state == "EXECUTER_ERROR")
                                {
                                    result = _subHelper.CloseSubmissionForTre(TesId.ToString(), StatusType.Failed, "", "");
                            }
                        }

                        // are we done ?
                        if (status.state == "COMPLETE" || status.state == "EXECUTOR_ERROR")
                            {
                                // Do this to avoid db locking issues
                                BackgroundJob.Enqueue(() => ClearJob(taskID));
                                // RecurringJob.RemoveIfExists(taskID);
                            }
                        }
                        else
                            Console.WriteLine("NO CHANGE " + status.state);
                  

                }
                else
                {
                    Console.WriteLine($"HTTP Request {url} failed with status code: {response.StatusCode}");
                }
            }

           
        }

        //Implement proper check
        private bool ValidateCreate(Submission sub)
        {
            return true;
        }

        // Method executed upon hangfire job
        public void Execute(bool useRabbit = true, bool useHutch = false, bool useTESK = true)
        {
            
            // control use of dependency injection
            using (var scope = _serviceProvider.CreateScope())
            {

                var cancelsubprojs =  _subHelper.GetRequestCancelSubsForTre();
                if (cancelsubprojs != null)
                {
                    foreach (var cancelsubproj in cancelsubprojs)
                    {
                        _subHelper.UpdateStatusForTre(cancelsubproj.Id.ToString(), StatusType.CancellationRequestSent, "");
                        //TODO Do we need to call Hutch or other stuff to cancel and do other cancel stuff
                        _subHelper.CloseSubmissionForTre(cancelsubproj.Id.ToString(), StatusType.Cancelled, "", "");
                    }
                    
                }

                // Get list of submissions
                List<Submission> listOfSubmissions;
               // var treApi = scope.ServiceProvider.GetRequiredService<ITreClientWithoutTokenHelper>();
                try
                {
                    listOfSubmissions = _subHelper.GetWaitingSubmissionForTre();
                }
                catch (Exception e)
                {
                   Log.Error("Error getting submissions: {message}", e.Message);
                   Console.WriteLine(e.Message);
                   throw;
                }

                Console.WriteLine("Number of submission = "+listOfSubmissions.Count);

                foreach (var aSubmission in listOfSubmissions)
                {
                    Log.Information("Submission: {submission}", aSubmission);

                    

                    // Check user is allowed ont he project
                    if (!_subHelper.IsUserApprovedOnProject(aSubmission.Project.Id, aSubmission.SubmittedBy.Id))
                    {
                        Log.Error("User {UserID}/project {ProjectId} is not value for this submission {submission}", aSubmission.SubmittedBy.Id, aSubmission.Project.Id, aSubmission);
                        // record error with submission layer
                        var result = _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.InvalidUser, "");
                        result = _subHelper.CloseSubmissionForTre(aSubmission.Id.ToString(), StatusType.Failed, "", "");
                    }
                    else
                    {


                        
                            Uri uri = new Uri(aSubmission.DockerInputLocation);
                            string fileName = Path.GetFileName(uri.LocalPath);
                            var sourceBucket = aSubmission.Project.SubmissionBucket;
                            var subProj = _dbContext.Projects.Where(x => x.SubmissionProjectId == aSubmission.Project.Id).FirstOrDefault();
                            
                                var destinationBucket = subProj.SubmissionBucketTre;
                                var source =  _minioSubHelper.GetCopyObject(sourceBucket, fileName);
                                var resultcopy =  _minioTreHelper.CopyObjectToDestination(destinationBucket, fileName, source.Result).Result;

                                _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.TreWaitingForCrateFormatCheck, "");
                        if (ValidateCreate(aSubmission))
                        {
                            _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.TreCrateValidated, "");
                        }
                        else
                        {
                            _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.SubmissionCrateValidationFailed, "");
                            _subHelper.CloseSubmissionForTre(aSubmission.Id.ToString(), StatusType.Failed, "", "");
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
                                EasyNetQ.Topology.Exchange exchangeObject = rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Submission, "topic");
                                rabbit.Advanced.Publish(exchangeObject, RoutingConstants.ProcessSub, false, new Message<TesTask>(tesMessage));
                            }
                            catch (Exception e)
                            {
                               Log.Error("Send rabbit failed : {message}",e.Message);
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
                                Log.Error("Send HUTCH failed : {message}", e.Message);
                                processedOK = false;
                            }
                        }

                        // **************  SEND TO TESK
                        if (useTESK)
                        {
                            if (tesMessage is not null)
                                CreateTESK(aSubmission.TesJson, aSubmission.TesId);
                        }

                        // **************  TELL SUBMISSION LAYER WE DONE
                        if (processedOK)
                        {
                            try
                            {
                                var result = _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.TransferredToPod, "");
                            }
                            catch (Exception e)
                            {
                                Log.Error("Send record outcome to submission layer : {message}", e.Message);
                                processedOK = false;
                            }
                         
                        }

                    }
                }
            }
            
        }

        public void ClearJob(string jobname)
        {
            Console.WriteLine("Hangfire clear job: "+jobname);
            RecurringJob.RemoveIfExists(jobname);
        }
    }
}