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

namespace TRE_API
{
    public interface IDoAgentWork
    {
        Task Execute();
        void CheckTESK(string taskID, int subId, string outputBucket);
        void ClearJob(string jobname);
        Task testing();
    }

    // TESK : http://172.16.34.31:8080/    https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks


    public class DoAgentWork : IDoAgentWork
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISubmissionHelper _subHelper;
        private readonly IHasuraAuthenticationService _hasuraAuthenticationService;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly AgentSettings _AgentSettings;
        private readonly MinioSettings _minioSettings;
        private readonly IMinioSubHelper _minioSubHelper;
        private readonly IMinioTreHelper _minioTreHelper;


        public DoAgentWork(IServiceProvider serviceProvider,
            ApplicationDbContext dbContext,
            ISubmissionHelper subHelper,
            IHasuraAuthenticationService hasuraAuthenticationService,
            IDareClientWithoutTokenHelper dareHelper,
            AgentSettings AgentSettings,
            MinioSettings minioSettings,
            IMinioTreHelper minioTreHelper,
            IMinioSubHelper minioSubHelper
            )
        {
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



        public async Task testing()
        {


            //TesId

            Console.WriteLine("Testing");
            string jsonContent = @"{
    ""name"": ""Hello World"",
    ""description"": ""Hello World, inspired by Funnel's most basic example"",
    ""executors"": [{
            ""image"": ""alpine"",
            ""command"": [""sleep"", ""5m""]
        }, {
            ""image"": ""alpine"",
            ""command"": [""echo"", ""TESK says:   Hello World""]
        }
    ],
	""outputs"" : [
		{
          ""path"": ""/data/outfile"",
          ""url"": ""s3://my-object-store/outfile-1"",
          ""type"": ""FILE""
        },
		{
          ""path"": ""/data/outfile2"",
          ""url"": ""s3://my-object-store/outfile-2"",
          ""type"": ""FILE""
        },
	],
    ""volumes"":null,
    ""tags"":{
        ""project"":""Head"",
        ""tres"":""SAIL|DPUK""
     },
    ""logs"":null,
    ""creation_time"":null
}
";
            CreateTESK(jsonContent, 0, "AAA");
        }

        public string CreateTESK(string jsonContent, int subId, string outputBucket)
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

                    RecurringJob.AddOrUpdate<IDoAgentWork>(id, a => a.CheckTESK(id, subId, outputBucket), Cron.MinuteInterval(1));

                    _dbContext.Add(new TeskAudit() { message = jsonContent, teskid = id });
                    _dbContext.SaveChanges();

                    return id;
                }
                else
                {
                    var darta = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    return "";
                }
            }
        }

        class ResponseModel
        {
            public string id { get; set; }
        }
        public async void CheckTESK(string taskID, int subId, string outputBucket)
        {
            Console.WriteLine("Check TESK : " + taskID + ",  TES : " + subId);

            string url = "https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks/" + taskID + "?view=basic";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;

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
                                    if (Token != null)
                                    {
                                        _dbContext.TokensToExpire.Remove(Token);
                                        _hasuraAuthenticationService.ExpirerToken(Token.Token);
                                    }
                                    _dbContext.SaveChanges();
                                    break;
                            }


                            var result = _subHelper.UpdateStatusForTre(subId, statusMessage, "");
                        }

                        // are we done ?
                        if (status.state == "COMPLETE" || status.state == "EXECUTOR_ERROR")
                        {
                            // Do this to avoid db locking issues
                            BackgroundJob.Enqueue(() => ClearJob(taskID));

                            var data = await _minioTreHelper.GetFilesInBucket(outputBucket);
                            var files = new List<string>();

                            foreach (var s3Object in data.S3Objects) //TODO is this right?
                            {
                                files.Add(s3Object.Key);
                            }
                                 

                            _subHelper.FilesReadyForReview(new ReviewFiles()
                            {
                                SubId = taskID, //TODO is this right  
                                Files = files
                            }); 


                            RecurringJob.RemoveIfExists(taskID);
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



        // Method executed upon hangfire job
        public async Task Execute()
        {

            // control use of dependency injection
            using (var scope = _serviceProvider.CreateScope())
            {
                // OPTIONS
                var useRabbit = _AgentSettings.UseRabbit;
                var useHutch = _AgentSettings.UseHutch;
                var useTESK = _AgentSettings.UseTESK;

                Console.WriteLine("Getting list of submissions");

                // Get list of submissions
                List<Submission> listOfSubmissions;
                // var treApi = scope.ServiceProvider.GetRequiredService<ITreClientWithoutTokenHelper>();
                try
                {
                    listOfSubmissions = await _subHelper.GetWaitingSubmissionForTre();
                    if (listOfSubmissions == null) return;
                }
                catch (Exception e)
                {
                    Log.Error("Error getting submissions: {message}", e.Message);
                    Console.WriteLine(e.Message);
                    throw;
                }

                Console.WriteLine("Number of submission = " + listOfSubmissions.Count);

                foreach (var aSubmission in listOfSubmissions)
                {
                    Log.Information("Submission: {submission}", aSubmission);



                    // Check user is allowed ont he project
                    if (!_subHelper.IsUserApprovedOnProject(aSubmission.Project.Id, aSubmission.SubmittedBy.Id))
                    {
                        Log.Error("User {UserID}/project {ProjectId} is not value for this submission {submission}", aSubmission.SubmittedBy.Id, aSubmission.Project.Id, aSubmission);
                        // record error with submission layer
                        //var result = _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.InvalidUser, "");
                    }
                    else
                    {



                        try
                        {

                            Uri uri = new Uri(aSubmission.DockerInputLocation);
                            string fileName = Path.GetFileName(uri.LocalPath);
                            var sourceBucket = aSubmission.Project.SubmissionBucket;
                            var subProj = _dbContext.Projects.Where(x => x.SubmissionProjectId == aSubmission.Project.Id);
                            foreach (var proj in subProj)
                            {
                                var destinationBucket = proj.SubmissionBucketTre;
                                var source = _minioSubHelper.GetCopyObject(sourceBucket, fileName);
                                var result = _minioTreHelper.CopyObjectToDestination(destinationBucket, fileName, source.Result).Result;

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
                                EasyNetQ.Topology.Exchange exchangeObject = rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Main, "topic");
                                rabbit.Advanced.Publish(exchangeObject, RoutingConstants.Subs, false, new Message<TesTask>(tesMessage));
                            }
                            catch (Exception e)
                            {
                                Log.Error("Send rabbit failed : {message}", e.Message);
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
                            var arr = new HttpClient();

                  

                            var role = aSubmission.Project.Name; //TODO Check

                            var Token = _hasuraAuthenticationService.GetNewToken(role);


                          
                            var projectId = aSubmission.Project.Id;

                           

                            var OutputBucket = _AgentSettings.TESKOutputBucketPrefix + _dbContext.Projects.First(x => x.Id == projectId).OutputBucketTre; //TODO Check, Projects not getting The synchronised Properly 
                            //it need the file name?? (key-name)


                            //S3://bucket-name/key-name
                            foreach (var output in tesMessage.Outputs)
                            {
                                output.Url = OutputBucket;
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
                                SubId = aSubmission.Id,
                                Token = Token
                            });
                            _dbContext.SaveChanges();

                            if (tesMessage is not null)
                                CreateTESK(JsonConvert.SerializeObject(tesMessage), aSubmission.Id, OutputBucket);
                        }

                        // **************  TELL SUBMISSION LAYER WE DONE
                        if (processedOK)
                        {
                            try
                            {
                                var result = _subHelper.UpdateStatusForTre(aSubmission.Id, StatusType.TransferredToPod, "");
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
            Console.WriteLine("Hangfire clear job: " + jobname);
            RecurringJob.RemoveIfExists(jobname);
        }
    }
}