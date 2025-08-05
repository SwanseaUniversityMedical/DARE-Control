using BL.Models;
using BL.Models.Enums;
using BL.Models.Settings;
using BL.Models.Tes;
using BL.Models.ViewModels;
using BL.Rabbit;
using BL.Services;
using EasyNetQ;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
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

        private static readonly HashSet<string> TerminalStates = new() { "COMPLETE", "EXECUTOR_ERROR", "SYSTEM_ERROR", "EXECUTER_ERROR" };

        public DoAgentWork(IServiceProvider serviceProvider,
            ApplicationDbContext dbContext,
            ISubmissionHelper subHelper,
            IMinioTreHelper minioTreHelper,
            IMinioSubHelper minioSubHelper,
            IHasuraAuthenticationService hasuraAuthenticationService,
            IDareClientWithoutTokenHelper dareHelper,
            AgentSettings agentSettings,
            MinioSettings minioSettings,
            IKeyCloakService keyCloakService,
            TreKeyCloakSettings treKeyCloakSettings,
            IEncDecHelper encDecHelper,
            IFeatureManager features
        )
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _subHelper = subHelper;
            _minioTreHelper = minioTreHelper;
            _minioSubHelper = minioSubHelper;
            _hasuraAuthenticationService = hasuraAuthenticationService;
            _dareHelper = dareHelper;
            _AgentSettings = agentSettings;
            _minioSettings = minioSettings;
            _keyCloakService = keyCloakService;
            _TreKeyCloakSettings = treKeyCloakSettings;
            _encDecHelper = encDecHelper;
            _features = features;
        }


        /// <summary>
        /// Called by Hangfire, each running job has a specific hangfire job that calls this method to check on the running status of that job periodically
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="subId"></param>
        /// <param name="tesId"></param>
        /// <param name="outputBucket"></param>
        /// <param name="NameTes"></param>
        /// <returns></returns>
        public async Task CheckTES(string taskID, int subId, string tesId, string outputBucket, string NameTes)
        {
            Log.Information("Checking TESK status {taskID}", taskID);
            using var httpClient = new HttpClient(CreateHttpHandler());
            var response = await httpClient.GetAsync($"{_AgentSettings.TESKAPIURL}/{taskID}?view=BASIC");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("TESK status check failed for {taskID}", taskID);
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<TESKstatus>(content);
            var existing = _dbContext.TESK_Status.FirstOrDefault(x => x.id == status.id);

            bool shouldReport = existing == null || existing.state != status.state;
            if (shouldReport)
            {
                if (existing == null)
                    _dbContext.Add(status);
                else { existing.state = status.state; _dbContext.Update(existing); }
                _dbContext.SaveChanges();
            }

            if (!shouldReport && !TerminalStates.Contains(status.state)) return;

            await HandleTESKFinalStatus(status.state, subId, taskID);

            if (TerminalStates.Contains(status.state))
                await FinalizeFilesAndNotify(subId, outputBucket, tesId, NameTes);
        }

        private async Task HandleTESKFinalStatus(string state, int subId, string taskID)
        {
            var token = _dbContext.TokensToExpire.FirstOrDefault(x => x.SubId == subId);
            if (token != null)
            {
                _dbContext.TokensToExpire.Remove(token);
                _hasuraAuthenticationService.ExpirerToken(token.Token);
                _dbContext.SaveChanges();
            }

            var statusType = state switch
            {
                "QUEUED" => StatusType.TransferredToPod,
                "RUNNING" => StatusType.PodProcessing,
                "COMPLETE" => StatusType.PodProcessingComplete,
                _ => StatusType.Cancelled
            };

            _subHelper.UpdateStatusForTre(subId.ToString(), statusType, "");
            if (state == "COMPLETE")
                _subHelper.CloseSubmissionForTre(subId.ToString(), StatusType.DataOutRequested, "", "");
            else if (state.Contains("ERROR"))
                _subHelper.CloseSubmissionForTre(subId.ToString(), StatusType.Failed, "", "");

            ClearJob(taskID);
        }

        private async Task FinalizeFilesAndNotify(int subId, string outputBucket, string tesId, string nameTes)
        {
            var cleaned = outputBucket.Replace(_AgentSettings.TESKOutputBucketPrefix, "");
            var data = await _minioTreHelper.GetFilesInBucket(cleaned, subId.ToString());
            var files = data.S3Objects.Select(f => f.Key).ToList();

            if (files.Count == 0)
            {
                _subHelper.UpdateStatusForTre(subId.ToString(), StatusType.DataOutApprovalRejected, "No files");
                return;
            }

            _subHelper.UpdateStatusForTre(subId.ToString(), StatusType.DataOutRequested, "");
            _subHelper.FilesReadyForReview(new ReviewFiles { SubId = subId.ToString(), Files = files, tesId = tesId, Name = nameTes }, cleaned);
        }

        public void ClearJob(string jobname)
        {
            try { RecurringJob.RemoveIfExists(jobname); }
            catch (Exception ex) { Log.Error(ex, "Clear job failed"); }
        }

        private HttpClientHandler CreateHttpHandler()
        {
            if (!_AgentSettings.Proxy) return new();
            return new HttpClientHandler { Proxy = new WebProxy(_AgentSettings.ProxyAddresURL, true), UseProxy = true };
        }

        private class ResponseModel { public string id { get; set; } }














        /// <summary>
        /// Method called by timed Hangfire job
        /// Purpose to communicate with the submission layer and get cancelled and new job to run
        /// </summary>

        public async Task Execute()
        {
            Log.Information("{Function} DoAgentWork running", nameof(Execute));
            using var scope = _serviceProvider.CreateScope();

            await HandleCancelledSubmissions();

            var submissions = _subHelper.GetWaitingSubmissionForTre() ?? new List<Submission>();
            foreach (var submission in submissions)
                await ProcessSubmission(submission);
        }


        private async Task HandleCancelledSubmissions()
        {
            var cancels = _subHelper.GetRequestCancelSubsForTre();
            if (cancels == null) return;

            foreach (var cancel in cancels)
            {
                Log.Information("Cancelling submission {SubId}", cancel.Id.ToString());
                _subHelper.UpdateStatusForTre(cancel.Id.ToString(), StatusType.CancellationRequestSent, "");
                _subHelper.CloseSubmissionForTre(cancel.Id.ToString(), StatusType.Cancelled, "", "");
            }
        }

        private async Task ProcessSubmission(Submission sub)
        {
            Log.Information("Processing submission {SubId}", sub.Id);
            if (!_subHelper.IsUserApprovedOnProject(sub.Project.Id, sub.SubmittedBy.Id))
            {
                _subHelper.UpdateStatusForTre(sub.Id.ToString(), StatusType.InvalidUser, "");
                _subHelper.CloseSubmissionForTre(sub.Id.ToString(), StatusType.Failed, "", "");
                return;
            }

            bool processedOK = true;

            if (await _features.IsEnabledAsync(FeatureFlags.DemoAllInOne))
            {
                try { _subHelper.SimulateSubmissionProcessing(sub); }
                catch (Exception e) { Log.Error(e, "Sim demo failed"); processedOK = false; }
            }

            var tesMessage = JsonConvert.DeserializeObject<TesTask>(sub.TesJson);
            if (_AgentSettings.UseRabbit) processedOK &= SendToRabbit(sub, tesMessage);
            if (_AgentSettings.UseTESK) processedOK &= await SendToTES(sub, tesMessage);

            if (processedOK)
                _subHelper.UpdateStatusForTre(sub.Id.ToString(), StatusType.TransferredToPod, "");
        }

        private bool SendToRabbit(Submission sub, TesTask msg)
        {
            try
            {
                var bus = _serviceProvider.GetRequiredService<IBus>();
                var exchange = bus.Advanced.ExchangeDeclare(ExchangeConstants.Submission, "topic");
                bus.Advanced.Publish(exchange, RoutingConstants.ProcessSub, false, new Message<TesTask>(msg));
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "RabbitMQ publish failed");
                return false;
            }
        }

        private async Task<bool> SendToTES(Submission sub, TesTask msg)
        {
            try
            {
                string token = await GetTokenForSubmission(sub);

                string outputBucket = _AgentSettings.TESKOutputBucketPrefix +
                    _dbContext.Projects.First(x => x.SubmissionProjectId == sub.Project.Id).OutputBucketTre;

                foreach (var o in msg.Outputs ??= new())
                    o.Url = $"{outputBucket}/{sub.Id}";

                foreach (var exec in msg.Executors ??= new())
                {
                    if (await _features.IsEnabledAsync(FeatureFlags.SqlAndNotGraphQl))
                    {
                        if (exec.Image.Contains(_AgentSettings.ImageNameToAddToToken))
                        {
                            exec.Env["TRINO_SERVER_URL"] = _AgentSettings.URLTrinoToAdd;
                            exec.Env["ACCESS_TOKEN"] = token;
                            exec.Env["USER_NAME"] = sub.SubmittedBy.Name;
                            exec.Env["SCHEMA"] = sub.Project.Name;
                            exec.Env["CATALOG"] = _AgentSettings.CATALOG;
                        }
                    }
                    else if (exec.Image.Contains(_AgentSettings.ImageNameToAddToTokenGraphQL))
                    {
                        exec.Command.Add("--Token_" + token);
                        exec.Command.Add("--URL_" + _AgentSettings.URLHasuraToAdd);
                    }
                }

                _dbContext.TokensToExpire.Add(new TokenToExpire { SubId = sub.Id, Token = token });
                _dbContext.SaveChanges();
                
                var json = JsonConvert.SerializeObject(msg);
                return await CreateTES(json, sub.Id, sub.TesId, outputBucket, sub.TesName) != "";
            }
            catch (Exception e)
            {
                Log.Error(e, "TESK submission failed");
                return false;
            }
        }

        private async Task<string> GetTokenForSubmission(Submission sub)
        {
            if (await _features.IsEnabledAsync(FeatureFlags.GenerateAccounts) &&
                await _features.IsEnabledAsync(FeatureFlags.SqlAndNotGraphQl))
            {
                var acc = _dbContext.ProjectAcount.FirstOrDefault(x => x.Name == sub.Project.Name + sub.SubmittedBy.Name);
                var tk = await _keyCloakService.GenAccessTokenSimple(acc.Name, _encDecHelper.Decrypt(acc.Pass), _TreKeyCloakSettings.TokenRefreshSeconds);
                return tk.access_token;
            }

            if (!await _features.IsEnabledAsync(FeatureFlags.SqlAndNotGraphQl))
                return _hasuraAuthenticationService.GetNewToken(sub.Project.Name);
            else
                return "";
        }

        public async Task<string> CreateTES(string jsonContent, int subId, string tesId, string outputBucket, string Tesname)
        {
            Log.Information("Creating TESK for sub {subId}", subId);

            using var httpClient = new HttpClient(CreateHttpHandler());
            var request = new HttpRequestMessage(HttpMethod.Post, _AgentSettings.TESKAPIURL)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("TESK request failed: {code}", response.StatusCode);
                return "";
            }

            var body = await response.Content.ReadAsStringAsync();
            var id = JsonConvert.DeserializeObject<ResponseModel>(body)?.id;

            RecurringJob.AddOrUpdate<IDoAgentWork>(id, a => a.CheckTES(id, subId, tesId, outputBucket, Tesname), Cron.MinuteInterval(1));

            _dbContext.Add(new TeskAudit { message = jsonContent, teskid = tesId, subid = subId.ToString() });
            _dbContext.SaveChanges();
            return id;
        }
        

      
    }
}