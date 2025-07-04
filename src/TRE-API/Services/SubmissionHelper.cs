using BL.Models;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Services;
using EasyNetQ;
using Serilog;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services.SignalR;
using static TRE_API.Controllers.SubmissionController;
using BL.Models.APISimpleTypeReturns;
using TRE_API.Models;


namespace TRE_API.Services
{
    public interface ISubmissionHelper
    {
        APIReturn? UpdateStatusForTre(string subId, StatusType statusType, string? description);
        void SimulateSubmissionProcessing(Submission submission);
        bool IsUserApprovedOnProject(int projectId, int userId);
        List<Submission>? GetWaitingSubmissionForTre();
        void SendSumissionToHUTCH(Submission submission);
        List<Submission>? GetRequestCancelSubsForTre();
        OutputBucketInfo GetOutputBucketGuts(string subId, bool hostnameonly, bool useExternal);
        APIReturn? CloseSubmissionForTre(string subId, StatusType statusType, string? description, string? finalFile);

        BoolReturn FilesReadyForReview(ReviewFiles review, string Bucketname);
        BoolReturn FilesReadyForReview(ReviewFiles review);

        OutputBucketInfo GetOutputBucketGutsSub(string subId, bool hostnameonly);
    }

    public class SubmissionHelper : ISubmissionHelper
    {
        private readonly IHutchClientHelper _hutchHelper;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly ApplicationDbContext _dbContext;
        private readonly MinioTRESettings _minioTreSettings;
        private readonly IBus _rabbit;
        private readonly IDataEgressClientWithoutTokenHelper _dataEgressHelper;
        private readonly IMinioTreHelper _minioTreHelper;
        private readonly AgentSettings _agentSettings;


        public string _hutchDbServer { get; set; }
        public string _hutchDbPort { get; set; }
        public string _hutchDbName { get; set; }


        public SubmissionHelper(ISignalRService signalRService,
            IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext,
            IBus rabbit,
            IHutchClientHelper hutchHelper,
            IConfiguration config,
            MinioTRESettings minioTreSettings,
            IDataEgressClientWithoutTokenHelper dataEgressHelper,
            IMinioTreHelper minioTreHelper,
            AgentSettings agentSettings
        )
        {
            _dareHelper = helper;
            _dbContext = dbContext;
            _rabbit = rabbit;
            _hutchHelper = hutchHelper;
            _hutchDbName = config["Hutch:DbName"];
            _hutchDbPort = config["Hutch:DbPort"];
            _hutchDbServer = config["Hutch:DbServer"];
            _minioTreSettings = minioTreSettings;

            _dataEgressHelper = dataEgressHelper;
            _minioTreHelper = minioTreHelper;
            _agentSettings = agentSettings;
        }

        public BoolReturn FilesReadyForReview(ReviewFiles review)
        {
            UpdateStatusForTre(review.SubId, StatusType.DataOutRequested, "");
            var bucket = GetOutputBucketGuts(review.SubId, false, false);
            var egsub = new EgressSubmission()
            {
                SubmissionId = review.SubId,
                OutputBucket = bucket.Bucket,
                Status = EgressStatus.NotCompleted,
                Files = new List<EgressFile>()
            };

            foreach (var reviewFile in review.Files)
            {
                egsub.Files.Add(new EgressFile()
                {
                    Name = reviewFile,
                    Status = FileStatus.Undecided
                });
            }

            var boolResult = _dataEgressHelper
                .CallAPI<EgressSubmission, BoolReturn>("/api/DataEgress/AddNewDataEgress/", egsub).Result;
            UpdateStatusForTre(review.SubId, StatusType.DataOutApprovalBegun, "");
            return boolResult;
        }

        public OutputBucketInfo GetOutputBucketGutsSub(string subId, bool hostnameonly)
        {
            try
            {
                var submission = _dareHelper
                    .CallAPIWithoutModel<Submission>($"/api/Submission/GetASubmission/{subId}")
                    .Result;

                string? outputBucket = "";

                if (_agentSettings.UseTESK == false)
                {
                    outputBucket = _dbContext.Projects
                        .Where(x => x.SubmissionProjectId == submission.Project.Id)
                        .Select(x => x.OutputBucketTre).FirstOrDefault();
                }
                else
                {
                    outputBucket = _dbContext.Projects
                        .Where(x => x.SubmissionProjectId == submission.Project.Id)
                        .Select(x => x.OutputBucketSub).FirstOrDefault();
                }


                bool secure = !_minioTreSettings.Url.ToLower().StartsWith("http://");
                return new OutputBucketInfo()
                {
                    Bucket = outputBucket ?? "",
                    SubId = submission.Id.ToString(),
                    Path = "sub" + subId + "/",
                    Secure = secure,
                    Host = hostnameonly
                        ? _minioTreSettings.Url.Replace("https://", "").Replace("http://", "")
                        : _minioTreSettings.Url
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "GetOutputBucketGuts");
                throw;
            }
        }


        //Use external is if something like hutch needs minio's external url and isn't sitting on same machine
        public OutputBucketInfo GetOutputBucketGuts(string subId, bool hostnameonly, bool useExternal)
        {
            try
            {
                Log.Information("{Function} Getting Bucket info", "GetOutputBucketInfo");
                var i = 1;
                var submission = _dareHelper
                    .CallAPIWithoutModel<Submission>($"/api/Submission/GetASubmission/{subId}")
                    .Result;

                var bucket = _dbContext.Projects
                    .Where(x => x.SubmissionProjectId == submission.Project.Id)
                    .Select(x => x.OutputBucketTre);

                var outputBucket = bucket.FirstOrDefault();
                var realurl = string.IsNullOrWhiteSpace(_minioTreSettings.HutchURLOverride)
                    ? _minioTreSettings.Url
                    : _minioTreSettings.HutchURLOverride;
                if (!useExternal)
                {
                    realurl = _minioTreSettings.Url;
                }

                bool secure = !_minioTreSettings.Url.ToLower().StartsWith("http://");
                return new OutputBucketInfo()
                {
                    Bucket = outputBucket ?? "",
                    SubId = submission.Id.ToString(),
                    Path = "sub" + subId + "/",
                    Secure = secure,
                    Host = hostnameonly ? realurl.Replace("https://", "").Replace("http://", "") : realurl
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "GetOutputBucketGuts");
                throw;
            }
        }

        public void SendSumissionToHUTCH(Submission submission)
        {
            Uri uri = new Uri(submission.DockerInputLocation);
            string fileName = Path.GetFileName(uri.LocalPath);
            var project = _dbContext.Projects.First(x => x.SubmissionProjectId == submission.Project.Id);
            var realurl = string.IsNullOrWhiteSpace(_minioTreSettings.HutchURLOverride)
                ? _minioTreSettings.Url
                : _minioTreSettings.HutchURLOverride;
            bool secure = !realurl.ToLower().StartsWith("http://");
            var job = new SubmitJobModel()
            {
                SubId = submission.Id.ToString(),

                DataAccess = new DatabaseConnectionDetails()
                {
                    Database = _hutchDbName,
                    Hostname = _hutchDbServer,
                    Username = project.UserName,
                    Password = project.Password,
                    Port = int.Parse(_hutchDbPort)
                },
                CrateSource = new FileStorageDetails()
                {
                    Bucket = project.SubmissionBucketTre,
                    Path = fileName,
                    Host = realurl.Replace("http://", "").Replace("https://", ""),
                    Secure = secure
                }
            };
            var statusParams = new Dictionary<string, string>()
            {
                { "subId", submission.Id.ToString() },
                { "statusType", StatusType.SendingSubmissionToHutch.ToString() },
                { "description", "" }
            };
            Log.Information("{Function} Minio url sent {Url} bucket {Bucket}, path {path}", "SendSubmissionToHutch",
                job.CrateSource.Host, job.CrateSource.Bucket, job.CrateSource.Path);
            var statusResult = _dareHelper
                .CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams).Result;
            var res = _hutchHelper.CallAPI<SubmitJobModel, JobStatusModel>($"/api/jobs/", job).Result;
        }

        public void SimulateSubmissionProcessing(Submission submission)
        {
            try
            {
                UpdateStatusForTre(submission.Id.ToString(), StatusType.Processing, "");
                UpdateStatusForTre(submission.Id.ToString(), StatusType.PreparingOutputs, "");
                UpdateStatusForTre(submission.Id.ToString(), StatusType.TransferredForDataOut, "");

                Uri uri = new Uri(submission.DockerInputLocation);
                string fileName = Path.GetFileName(uri.LocalPath);

                var destinationBucket = GetOutputBucketGuts(submission.Id.ToString(), false, false);
                var subProj = _dbContext.Projects
                    .FirstOrDefault(x => x.SubmissionProjectId == submission.Project.Id);
                var sourceBucket = subProj.SubmissionBucketTre;
                Log.Information("{Function} Copying {File} from {From} to {To}", "Execute", fileName, sourceBucket,
                    destinationBucket);
                var source = _minioTreHelper.GetCopyObject(sourceBucket, fileName);
                var resultcopy = _minioTreHelper
                    .CopyObjectToDestination(destinationBucket.Bucket, fileName, source.Result).Result;
                Log.Information("{Function} Simulate submission for Id {Id} returned {Result}",
                    "SimulateSubmissionProcessing", submission.Id, resultcopy);
                var reviewFiles = new ReviewFiles()
                {
                    Files = new List<string>() { fileName },
                    SubId = submission.Id.ToString(),
                    tesId = submission.TesId
                };
                var result = FilesReadyForReview(reviewFiles);
            }
            catch (Exception e)
            {
                Log.Error(e, "{Function} Something went wrong with submission {Id}", "SimulateSubmissionProcessing",
                    submission.Id);
                throw;
            }
        }

        public APIReturn? UpdateStatusForTre(string subId, StatusType statusType, string? description)
        {
            try
            {
                Log.Information($"UpdateStatusForTre subId {subId} statusType {statusType} description {description} ");
                var result = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                        new Dictionary<string, string>()
                        {
                            { "subId", subId }, { "statusType", statusType.ToString() }, { "description", description }
                        })
                    .Result;
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
        }

        public APIReturn? CloseSubmissionForTre(string subId, StatusType statusType, string? description,
            string? finalFile)
        {
            Log.Information(
                $"CloseSubmissionForTre subId {subId} statusType {statusType} description {description} finalFile {finalFile}");
            var result = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/CloseSubmissionForTre",
                    new Dictionary<string, string>()
                    {
                        { "subId", subId }, { "statusType", statusType.ToString() }, { "description", description },
                        { "finalFile", finalFile }
                    })
                .Result;

            return result;
        }


        public bool IsUserApprovedOnProject(int projectId, int userId)
        {
            return _dbContext.MembershipDecisions.Any(x =>
                x.Project != null && x.Project.SubmissionProjectId == projectId && x.User != null &&
                x.User.SubmissionUserId == userId &&
                !x.Project.Archived && x.Project.Decision == Decision.Approved && !x.Archived &&
                x.Decision == Decision.Approved);
        }

        public List<Submission>? GetWaitingSubmissionForTre()
        {
            if (_dbContext.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Submission))
            {
                var result =
                    _dareHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForTre")
                        .Result;
                return result;
            }
            else
            {
                return new List<Submission>();
            }
        }

        public List<Submission>? GetRequestCancelSubsForTre()
        {
            if (_dbContext.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Submission))
            {
                var result =
                    _dareHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetRequestCancelSubsForTre")
                        .Result;
                return result;
            }
            else
            {
                return new List<Submission>();
            }
        }

        public BoolReturn FilesReadyForReview(ReviewFiles review, string Bucketname)
        {
            var egsub = new EgressSubmission()
            {
                SubmissionId = review.SubId,
                OutputBucket = Bucketname,
                Status = EgressStatus.NotCompleted,
                Files = new List<EgressFile>(),
                tesId = review.tesId,
                Name = review.Name,
            };

            foreach (var reviewFile in review.Files)
            {
                egsub.Files.Add(new EgressFile()
                {
                    Name = reviewFile,
                    Status = FileStatus.Undecided
                });
            }

            Log.Information(
                $"FilesReadyForReview egsub.Files.Count {egsub.Files.Count} egsub.OutputBucket {egsub.OutputBucket} ");
            var boolResult = _dataEgressHelper
                .CallAPI<EgressSubmission, BoolReturn>("/api/DataEgress/AddNewDataEgress/", egsub).Result;
            return boolResult;
        }
    }
}