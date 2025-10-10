using BL.Models;
using BL.Models.Enums;
using BL.Models.Settings;
using BL.Models.Tes;
using BL.Models.ViewModels;
using BL.Rabbit;
using BL.Services;
using EasyNetQ;
using Hangfire;
using Microsoft.FeatureManagement;
using Newtonsoft.Json;
using Serilog;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TRE_API.Constants;
using TRE_API.Models;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;
using TREAgent.Repositories;

namespace TRE_API
{
    public interface IDoAgentWork
    {
        Task Execute();
        Task CheckTES(string taskID, int subId, string tesId, string outputBucket, string NameTes);
        void ClearJob(string jobname);
    }

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
        private readonly IKeyCloakService _keyCloakService;
        private readonly TreKeyCloakSettings _TreKeyCloakSettings;
        private readonly IEncDecHelper _encDecHelper;
        private readonly IFeatureManager _features;


        public DoAgentWork(IServiceProvider serviceProvider,
            ApplicationDbContext dbContext,
            ISubmissionHelper subHelper,
            IMinioTreHelper minioTreHelper,
            IMinioSubHelper minioSubHelper,
            IHasuraAuthenticationService hasuraAuthenticationService,
            IDareClientWithoutTokenHelper dareHelper,
            AgentSettings AgentSettings,
            MinioSettings minioSettings,
            IKeyCloakService keyCloakService,
            TreKeyCloakSettings TreKeyCloakSettings,
            IEncDecHelper encDecHelper,
            IFeatureManager features
        )
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

            _keyCloakService = keyCloakService;
            _TreKeyCloakSettings = TreKeyCloakSettings;
            _encDecHelper = encDecHelper;
            _features = features;
        }

        public string CreateTesk(string jsonContent, int subId, string tesId, string outputBucket, string Tesname)
        {
            Log.Information("{Function} {jsonContent} running CreateTESK ", "CreateTesk", jsonContent);

            HttpClientHandler handler = new HttpClientHandler();

            if (_AgentSettings.Proxy)
            {
                handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(_AgentSettings.ProxyAddresURL, true), // Replace with your proxy server URL
                    UseProxy = _AgentSettings.Proxy,
                };
            }


            using var httpClient = new HttpClient(handler);
            // Define the URL for the POST request
            string apiUrl = _AgentSettings.TESKAPIURL;

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
                Log.Information("{Function} Request successful. Response: {Response}", "CreateTesk", responseBody);
                Console.WriteLine("Request successful. Response:");
                Console.WriteLine(responseBody);
                Log.Information("Request successful. Response: {response}", responseBody);


                var responseObj = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                string id = responseObj.id;


                RecurringJob.AddOrUpdate<IDoAgentWork>(id,
                    a => a.CheckTES(id, subId, tesId, outputBucket, Tesname),
                    Cron.Minutely());


                _dbContext.Add(new TeskAudit() { message = jsonContent, teskid = tesId, subid = subId.ToString() });
                _dbContext.SaveChanges();


                return id;
            }
            try
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                Log.Error("{Function} Request failed with status code: {Code} {responseBody}", "CreateTESK", response.StatusCode, responseBody);
            }
            catch (Exception ex) {
                Log.Error("{Function} Request failed with status code: {Code}", "CreateTESK", response.StatusCode);
            }
            


            return "";
        }


        class ResponseModel
        {
            public string id { get; set; }
        }

        public async Task CheckTES(string taskID, int subId, string tesId, string outputBucket, string NameTes)
        {
            try
            {
                Log.Information("{Function} Check TES : {TaskId},  TES : {TesId}, sub: {SubId}", "CheckTES", taskID,
                    tesId, subId);
                string url = _AgentSettings.TESKAPIURL + "/" + taskID + "?view=BASIC";

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
                    Log.Information("{Function} Response status {State}", "CheckTES", response.StatusCode);
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
                        Log.Information("{Function} shouldReport {shouldReport} status {status}", "CheckTES",
                            shouldReport, status.state);
                        if (shouldReport || (status.state == "COMPLETE" || status.state == "EXECUTOR_ERROR" ||
                                             status.state == "SYSTEM_ERROR"))
                        {
                            Log.Information("{Function} *** status change *** {State} {name} {description}", "CheckTES", status.state,status.name ,  status.description);


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
                                        Log.Information("{Function} *** COMPLETE remove Token *** {Token} ",
                                            "CheckTES", Token);

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
                                        Log.Information("{Function} *** EXECUTOR_ERROR remove Token *** {Token} ",
                                            "CheckTES", Token);

                                        if (Token != null)
                                        {
                                            _dbContext.TokensToExpire.Remove(Token);
                                            _hasuraAuthenticationService.ExpirerToken(Token.Token);
                                        }

                                        _dbContext.SaveChanges();

                                        break;
                                    case "SYSTEM_ERROR":
                                        statusMessage = StatusType.Cancelled;
                                        Token = _dbContext.TokensToExpire.FirstOrDefault(x => x.SubId == subId);
                                        Log.Information("{Function} *** SYSTEM_ERROR remove Token *** {Token} ",
                                            "CheckTES", Token);

                                        if (Token != null)
                                        {
                                            _dbContext.TokensToExpire.Remove(Token);
                                            _hasuraAuthenticationService.ExpirerToken(Token.Token);
                                        }

                                        _dbContext.SaveChanges();

                                        break;
                                }

                                APIReturn? result = null;
 

                                if (status.state == "COMPLETE")
                                {
                                    Log.Information(
                                        $"  CloseSubmissionForTre with status.state subId {subId.ToString()} == COMPLETE ");
                                    try
                                    {
                                        result = _subHelper.CloseSubmissionForTre(subId.ToString(),
                                            StatusType.DataOutRequested, "", "");
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error(ex.ToString());
                                    }

                                    ClearJob(taskID);
                                }
                                else if (status.state == "EXECUTOR_ERROR" || status.state == "SYSTEM_ERROR")
                                {
                                    Log.Information(
                                        $"  CloseSubmissionForTre with status.state subId {subId.ToString()} == EXECUTOR_ERROR or SYSTEM_ERROR ");
                                    try
                                    {
                                        result = _subHelper.CloseSubmissionForTre(subId.ToString(), StatusType.Failed,
                                            "", "");
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error(ex.ToString());
                                    }

                                    ClearJob(taskID);
                                }
                            }

                            Log.Information($" Checking status ");
                            // are we done ?
                            if (status.state == "COMPLETE")
                            {
                                Log.Information(
                                    "status.state == COMPLETE");

                                ClearJob(taskID);
                                var outputBucketGood = outputBucket.Replace(_AgentSettings.TESKOutputBucketPrefix, "");
                                var data = await _minioTreHelper.GetFilesInBucket(outputBucketGood, $"{subId}");
                                var files = new List<string>();

                                foreach (var s3Object in data.S3Objects) //TODO is this right?
                                {
                                    Log.Information("{Function} *** added file from outputBucket *** {file} ",
                                        "CheckTES", s3Object.Key);
                                    files.Add(s3Object.Key);
                                }

                                _subHelper.UpdateStatusForTre(subId.ToString(), StatusType.DataOutRequested, "");
                                Log.Information($"  FilesReadyForReview files {files.Count} ");
                                if (files.Count == 0)
                                {
                                    _subHelper.UpdateStatusForTre(subId.ToString(), StatusType.DataOutApprovalRejected,
                                        " No Files to review ");
                                    return;
                                }

                                _subHelper.FilesReadyForReview(new ReviewFiles()
                                {
                                    SubId = subId.ToString(),
                                    Files = files,
                                    tesId = tesId.ToString(),
                                    Name = NameTes
                                }, outputBucketGood);
                            }
                        }
                        else
                            Log.Information("{Function} No change", "CheckTES");
                    }
                    else
                    {
                        Log.Error("{Function} HTTP Request {url} failed with status code {code}", "CheckTES", url,
                            response.StatusCode);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        // Method executed upon hangfire job
        public async Task Execute()
        {
            Log.Information("{Function} DoAgentWork running", "Execute");
            // control use of dependency injection
            using (var scope = _serviceProvider.CreateScope())
            {
                // OPTIONS
                var useRabbit = _AgentSettings.UseRabbit;
                var useTESK = _AgentSettings.UseTESK;

                Log.Information("{Function} useRabbit {useRabbit}", "Execute", useRabbit);
                Log.Information("{Function} useTESK {useTESK}", "Execute", useTESK);
                if (await _features.IsEnabledAsync(FeatureFlags.DemoAllInOne))
                {
                    Log.Information("{Function} Demo Mode is on, simulating execution..", "Execute");
                    
                }

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
                    Log.Error(e, "{Function} Error getting submissions", "Execute");

                    throw;
                }


                Log.Information("{Function} listOfSubmissions {listOfSubmissions}", "Execute",
                    listOfSubmissions?.Count);
                foreach (var aSubmission in listOfSubmissions)
                {
                    

                    try
                    {
                        Log.Information("{Function}Submission: {submission}", "Execute", aSubmission.Id);

                        // Check user is allowed on the project
                        if (!_subHelper.IsUserApprovedOnProject(aSubmission.Project.Id, aSubmission.SubmittedBy.Id))
                        {
                            Log.Error(
                                "{Function }User {UserID}/project {ProjectId} is not value for this submission {submission}",
                                "Execute",
                                aSubmission.SubmittedBy.Id, aSubmission.Project.Id, aSubmission);
                            // record error with submission layer
                            var result =
                                _subHelper.UpdateStatusForTre(aSubmission.Id.ToString(), StatusType.InvalidUser, "");
                            result = _subHelper.CloseSubmissionForTre(aSubmission.Id.ToString(), StatusType.Failed, "",
                                "");
                        }
                        else
                        {
                             


                            // The TES message
                            var tesMessage = JsonConvert.DeserializeObject<TesTask>(aSubmission.TesJson);
                            var processedOK = true;
                            if (await _features.IsEnabledAsync(FeatureFlags.DemoAllInOne))
                            {
                                try
                                {
                                    _subHelper.SimulateSubmissionProcessing(aSubmission);
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e, "{Function} Simulation failed for sub {SubId}", "Execute",
                                        aSubmission.Id);
                                    processedOK = false;
                                }
                            }   

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
                                    Log.Error(e, "{Function} Send rabbit failed for sub {SubId}", "Execute",
                                        aSubmission.Id);
                                    processedOK = false;
                                }
                            }
 
                            // **************  SEND TO TESK
                            if (useTESK)
                            {
                                Log.Information("{Function}  SEND TO TESK ", "Execute");
                                var arr = new HttpClient();
                                var Token = "";

                                var role = aSubmission.Project.Name; //TODO Check

                                if (await _features.IsEnabledAsync(FeatureFlags.GenerateAccounts) &&
                                    await _features.IsEnabledAsync(FeatureFlags.SqlAndNotGraphQl))
                                {
                                    var Acount = _dbContext.ProjectAcount.FirstOrDefault(x =>
                                        x.Name == aSubmission.Project.Name + aSubmission.SubmittedBy.Name);

                                    var TokenIN = await _keyCloakService.GenAccessTokenSimple(Acount.Name,
                                        _encDecHelper.Decrypt(Acount.Pass), _TreKeyCloakSettings.TokenRefreshSeconds);

                                    Token = TokenIN.access_token;
                                }

                                if (!await _features.IsEnabledAsync(FeatureFlags.SqlAndNotGraphQl))
                                {
                                    Token = _hasuraAuthenticationService.GetNewToken(role);
                                }

                                var projectId = aSubmission.Project.Id;

                                var OutputBucket = _AgentSettings.TESKOutputBucketPrefix + _dbContext.Projects
                                    .First(x => x.SubmissionProjectId == projectId)
                                    .OutputBucketTre; //TODO Check, Projects not getting The synchronised Properly 


                                //it need the file name?? (key-name)

                                if (tesMessage.Outputs == null)
                                {
                                    tesMessage.Outputs = new List<TesOutput> { };
                                }

                               



                                //S3://bucket-name/key-name
                                foreach (var output in tesMessage.Outputs)
                                {
                                    output.Url = OutputBucket + $"/{aSubmission.Id}";

                                }


                                var InputBucket = _dbContext.Projects
                                  .First(x => x.SubmissionProjectId == projectId)
                                  .SubmissionBucketTre;

                                var bucket = _subHelper.GetOutputBucketGutsSub(aSubmission.Id.ToString(), true);

                                if (tesMessage.Inputs == null)
                                {
                                    tesMessage.Inputs = new List<TesInput>();
                                }
                                if (string.IsNullOrEmpty(_AgentSettings.MandatoryInput) == false)
                                {
                                    tesMessage.Inputs.Add(JsonConvert.DeserializeObject<TesInput>(_AgentSettings.MandatoryInput));
                                }
                                

                                var Files = await _minioTreHelper.GetFilesInBucket(InputBucket);

                                foreach (var input in tesMessage.Inputs)
                                {
                                    //+ _AgentSettings.TESKinputBucketIP + "/"

                                    input.Path = input.Path.Replace("..", "");
                                    input.Url = "s3://" + InputBucket + input.Path;
                                    var CleanedIntput = input.Path;

                                    if (input.Path.StartsWith("/"))
                                    {
                                        CleanedIntput = CleanedIntput.Remove(0, 1);
                                    }

                                    if (string.IsNullOrEmpty(input.Name))
                                    {
                                        if (input.Path.Contains("/"))
                                        {
                                            input.Name = input.Path.Split('/')[^1];
                                        }
                                    }

                                    
                                    var source = await _minioSubHelper.GetCopyObject(aSubmission.Project.SubmissionBucket, CleanedIntput);

                                    if (Files.S3Objects.Any(x => x.ETag == source.ETag))
                                    {
                                        continue;
                                    }

                                    var resultcopy = await _minioTreHelper.CopyObjectToDestination(InputBucket, CleanedIntput, source);

                                }


                                if (tesMessage.Executors == null)
                                {
                                    tesMessage.Executors = new List<TesExecutor>();
                                }

                                Log.Information("looking for _AgentSettings.ImageNameToAddToToken > " +
                                                _AgentSettings.ImageNameToAddToToken);
                                foreach (var Executor in tesMessage.Executors)
                                {
                                    Log.Information("Executor.Image > " + Executor.Image);
                                    if (await _features.IsEnabledAsync(FeatureFlags.SqlAndNotGraphQl))
                                    {
                                        if (Executor.Image.Contains(_AgentSettings.ImageNameToAddToToken))
                                        {


                                            Executor.Env["TRINO_SERVER_URL"] = _AgentSettings.URLTrinoToAdd;
                                            Executor.Env["ACCESS_TOKEN"] = Token;
                                            Executor.Env["USER_NAME"] = aSubmission.SubmittedBy.Name;
                                            Executor.Env["SCHEMA"] = aSubmission.Project.Name;
                                            Executor.Env["CATALOG"] = _AgentSettings.CATALOG;

                                            if (string.IsNullOrEmpty(Executor.Env["TRINO_SERVER_URL"]))
                                            {
                                                Executor.Env["TRINO_SERVER_URL"] = "";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Executor.Image.Contains(_AgentSettings.ImageNameToAddToTokenGraphQL))
                                        {
                                            Executor.Command.Add("--Token_" + Token);
                                            Executor.Command.Add("--URL_" + _AgentSettings.URLHasuraToAdd);
                                        }
                                    }
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
                                    Log.Information("{Function} tesMessage is not null runhing CreateTESK {tesMessage}",
                                        "Execute", stringdata);

                                    CreateTesk(stringdata, aSubmission.Id, aSubmission.TesId, OutputBucket,
                                        aSubmission.TesName);
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
                                    Log.Error(e,
                                        "{Function} Error sending record outcome to submission layer for sub {SubId}",
                                        "Execute", aSubmission.Id);
                                    processedOK = false;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "{Function } Error occured processing submission {SubId}", "Execute",
                            aSubmission.Id);
                    }
                }
            }
        }

        public void ClearJob(string jobname)
        {
            Log.Information("{Function} Hangfire clear job: {Jobname}", "ClearJob", jobname);

            try
            {
                RecurringJob.RemoveIfExists(jobname);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}