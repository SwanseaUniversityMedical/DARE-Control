using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.FeatureManagement;
using Serilog;
using System.Text;
using TRE_API.Constants;
using TRE_API.Repositories.DbContexts;
using Tre_Credentials.DbContexts;


namespace TRE_API.Services
{
    public class DareSyncHelper : IDareSyncHelper
    {
        public ApplicationDbContext _DbContext { get; set; }

        public CredentialsDbContext _CredentialsDbContext { get; set; }
        public IDareClientWithoutTokenHelper _dareclientHelper { get; set; }
        
        private readonly IMinioTreHelper _minioTreHelper;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IConfiguration _config;

        private readonly IFeatureManager _features;

        public DareSyncHelper(ApplicationDbContext dbContext, IDareClientWithoutTokenHelper dareClient,  IMinioTreHelper minioTreHelper, CredentialsDbContext credentialsDbContext, IHttpClientFactory httpClientFactory, IConfiguration config, IFeatureManager features)
        {
            _DbContext = dbContext;
            _dareclientHelper = dareClient;
            
            _minioTreHelper = minioTreHelper;

            _CredentialsDbContext = credentialsDbContext;

            _httpClientFactory = httpClientFactory;

            _features = features;

            _config = config;
        }

        public async Task<BoolReturn> SyncSubmissionWithTre()
        {
            if (!_dareclientHelper.CheckCredsAreAvailable())
            {
                Log.Error("{Function} Credential not yet entered for synching with Dare", "SyncSubmissionWithTre");
                return new BoolReturn()
                {
                    Result = false
                };
            }

            if(await _features.IsEnabledAsync(FeatureFlags.EphemeralCredentials))
            {
                var waitingSubs = await _dareclientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForTre");

                foreach (var sub in waitingSubs)
                {
                    //This piece of code allows to skip if the creds are already present in the creds DB
                    var alreadyTriggered = _CredentialsDbContext.EphemeralCredentials.Any(c => c.SubmissionId == sub.Id);
                    if (alreadyTriggered) continue;

                    var projectName = sub.Project.Name;
                    var userId = sub.SubmittedBy.Id;
                    var submissionId = sub.Id;

                    try
                    {
                        await TriggerStartCredentialsAsync(submissionId, projectName, userId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to trigger Camunda for Sub {Sub}", submissionId);
                    }
                }
            }
            else
            {
                Log.Information("Ephemeral Credentials feature flag is disabled; skipping credential triggering.");
            }


            var subprojs = await _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre");
            var dbprojs = _DbContext.Projects.ToList();
            var projectAdds = subprojs.Where(x => !_DbContext.Projects.Any(y => y.SubmissionProjectId == x.Id));
            var projectArchives =
                dbprojs.Where(x => !subprojs.Any(y => y.Id == x.SubmissionProjectId));
            var projectUnarchives = dbprojs.Where(x => x.Archived && subprojs.Any(y => y.Id == x.SubmissionProjectId));
            foreach (var project in projectAdds)
            {

                var submission = project.SubmissionBucket.ToLower() + "tre".Replace("_", "");
                var output = project.OutputBucket.ToLower() + "tre".Replace("_", "");
                var submissionBucket = await _minioTreHelper.CreateBucket(submission);
                if (!submissionBucket)
                {
                    Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "SyncSubmissionWithTre", submission);
                    submission = "";
                }

                var outputBucket = await _minioTreHelper.CreateBucket(output);
                if (!outputBucket)
                {
                    Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "SyncSubmissionWithTre", output);
                    output = "";
                }

                _DbContext.Projects.Add(new TreProject()
                {
                    SubmissionProjectId = project.Id,
                    SubmissionProjectName = project.Name,
                    Description = project.ProjectDescription,
                    SubmissionBucketTre = submission,
                    OutputBucketTre = output,
                    OutputBucketSub = project.OutputBucket.ToLower(),
                    ProjectExpiryDate = project.EndDate,
                });

            }

            foreach (var treProject in projectArchives)
            {
                treProject.Archived = true;
                foreach (var treProjectMemberDecision in treProject.MemberDecisions)
                {
                    treProjectMemberDecision.Archived = true;
                }
            }

            foreach (var projectUnarchive in projectUnarchives)
            {
                projectUnarchive.Archived = false;
            }

            await _DbContext.SaveChangesAsync();
            var users = subprojs.SelectMany(x => x.Users).Distinct();
            var dbusers = _DbContext.Users.ToList();
            var userAdds = users.Where(x => !_DbContext.Users.Any(y => y.SubmissionUserId == x.Id));
            var userArchives = dbusers.Where(x => !users.Any(y => y.Id == x.SubmissionUserId));
            var userUnarchives = dbusers.Where(x => x.Archived && users.Any(y => y.Id == x.SubmissionUserId));
            foreach (var user in userAdds)
            {
                _DbContext.Users.Add(new TreUser()
                {
                    SubmissionUserId = user.Id,
                    Username = user.Name,
                    Email = user.Email,
                });
            }

            foreach (var userArchive in userArchives)
            {
                userArchive.Archived = true;
            }

            foreach (var userUnarchive in userUnarchives)
            {
                userUnarchive.Archived = false;
            }
            await _DbContext.SaveChangesAsync();
            var projectUserPairs = subprojs
                .SelectMany(project => project.Users, (project, user) => new
                {
                    ProjectId = project.Id,
                    UserId = user.Id
                }).ToList();
            var dbmembers = _DbContext.MembershipDecisions.ToList();
            var memberAdds = projectUserPairs.Where(x => !_DbContext.MembershipDecisions.Any(y =>
                y.Project.SubmissionProjectId == x.ProjectId && y.User.SubmissionUserId == x.UserId));
            var memberArchives = dbmembers.Where(x =>
                !projectUserPairs.Any(y =>
                    y.ProjectId == x.Project.SubmissionProjectId && y.UserId == x.User.SubmissionUserId));
            var memberUnarchives = dbmembers.Where(x =>
                x.Archived && projectUserPairs.Any(y =>
                    y.ProjectId == x.Project.SubmissionProjectId && y.UserId == x.User.SubmissionUserId));

            foreach (var memberAdd in memberAdds)
            {
                var project = _DbContext.Projects.First(x => x.SubmissionProjectId == memberAdd.ProjectId);
                var user = _DbContext.Users.First(x => x.SubmissionUserId == memberAdd.UserId);
                _DbContext.MembershipDecisions.Add(new TreMembershipDecision()
                {
                    User = user,
                    Project = project,
                    ProjectExpiryDate = project.ProjectExpiryDate
                });
            }

            foreach (var treMembershipDecision in memberArchives)
            {
                treMembershipDecision.Archived = true;
            }

            foreach (var treMembershipDecision in memberUnarchives)
            {
                treMembershipDecision.Archived = false;
            }
          
            await _DbContext.SaveChangesAsync();
            await SyncProjectDecisions();
            await SyncMembershipDecisions();

            return new BoolReturn()
            {
                Result = true
            };

        }

        public async Task<bool> SyncProjectDecisions()
        {

            var dbprojs = _DbContext.Projects.ToList();            
            var synclist = dbprojs.Select(x => new ProjectTreDecisionsDTO() { ProjectId = x.SubmissionProjectId, Decision = x.Decision }).ToList();
            var result = await _dareclientHelper.CallAPI<List<ProjectTreDecisionsDTO>, BoolReturn>("/api/Project/SyncTreProjectDecisions", synclist);
            


            return result.Result;

        }

        public async Task<bool> SyncMembershipDecisions()
        {

            var dbMemberships = _DbContext.MembershipDecisions.ToList();



            var synclist = dbMemberships.Select(x => new MembershipTreDecisionDTO() { ProjectId = x.Project.SubmissionProjectId, UserId = x.User.SubmissionUserId, Decision = x.Decision }).ToList();
            var result = await _dareclientHelper.CallAPI<List<MembershipTreDecisionDTO>, BoolReturn>("/api/Project/SyncTreMembershipDecisions", synclist);



            return result.Result;

        }

        private async Task TriggerStartCredentialsAsync(int submissionId, string projectName, int userId)
        {
            var payload = new
            {
                records = new[]
                {
                    new
                    {
                        project = projectName,
                        user = userId.ToString(),
                        submissionId = submissionId.ToString()

                    }
                }
            };

            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var camundaWebhookUrl = _config["CredentialAPISettings:StartWebhookUrl"];

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(2);

            var response = await httpClient.PostAsync(camundaWebhookUrl, content);                      

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Error("Camunda webhook call failed for submission {SubmissionId}. Error: {Error}", submissionId, error);
                throw new Exception($"Camunda webhook call failed: {response.StatusCode}");
            }

            Log.Information("Camunda StartCredentials triggered successfully for submission {SubmissionId}", submissionId);
        }      
    }
}
