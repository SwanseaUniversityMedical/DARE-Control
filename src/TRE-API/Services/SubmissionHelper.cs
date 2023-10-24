using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Services;
using EasyNetQ;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services.SignalR;
using static TRE_API.Controllers.SubmissionController;

namespace TRE_API.Services
{

    public interface ISubmissionHelper
    {
        APIReturn? UpdateStatusForTre(int subId, StatusType statusType, string? description);
        bool IsUserApprovedOnProject(int projectId, int userId);
        Task<List<Submission>?> GetWaitingSubmissionForTre();
        void SendSumissionToHUTCH(Submission submission);

        BoolReturn FilesReadyForReview(ReviewFiles review);

        OutputBucketInfo GetOutputBucketGuts(string subId);

    }
    public class SubmissionHelper: ISubmissionHelper
    {
        private readonly IHutchClientHelper _hutchHelper;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly ApplicationDbContext _dbContext;
        private readonly MinioTRESettings _minioTreSettings;
        private readonly IBus _rabbit;
        private readonly IDataEgressClientWithoutTokenHelper _dataEgressHelper;
        private readonly IMinioTreHelper _minioTreHelper;

        public string _hutchDbServer { get; set; }
        public string _hutchDbPort { get; set; }
        public string _hutchDbName { get; set; }


        public SubmissionHelper(ISignalRService signalRService, IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext, IBus rabbit, IHutchClientHelper hutchHelper,
            IConfiguration config, MinioTRESettings minioTreSettings,
           IDataEgressClientWithoutTokenHelper dataEgressHelper,
           IMinioTreHelper minioTreHelper)
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

        }

        public void SendSumissionToHUTCH(Submission submission)
        {
            Uri uri = new Uri(submission.DockerInputLocation);
            string fileName = Path.GetFileName(uri.LocalPath);
            var project = _dbContext.Projects.First(x => x.SubmissionProjectId == submission.Project.Id);
            var job = new SubmitJobModel()
            {
                JobId = submission.Id.ToString(),
                
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
                    Host = _minioTreSettings.Url
                }
                
            };
            var statusParams = new Dictionary<string, string>()
            {
                { "subId", submission.Id.ToString() },
                { "statusType", StatusType.SendingFileToHUTCH.ToString() },
                { "description", "" }
            };
            

            var res = _hutchHelper.CallAPI<SubmitJobModel, JobStatusModel>($"/api/jobs/", job).Result;

            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams).Result;
        }

        public APIReturn? UpdateStatusForTre(string subId, StatusType statusType, string? description)
        {
            var result = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                    new Dictionary<string, string>()
                        { { "subId", subId }, { "statusType", statusType.ToString() }, { "description", description } })
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

        public async Task<List<Submission>?> GetWaitingSubmissionForTre()
        {
            var result = await _dareHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForTre");
            return result;
        }
        public BoolReturn FilesReadyForReview(ReviewFiles review)
        {
            var bucket = GetOutputBucketGuts(review.SubId);
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
            var boolResult = _dataEgressHelper.CallAPI<EgressSubmission, BoolReturn>("/api/DataEgress/AddNewDataEgress/", egsub).Result;
            return boolResult;
        }

        public OutputBucketInfo GetOutputBucketGuts(string subId)
        {

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("submissionId", subId.ToString());
            var submission = _dareHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist)
                .Result;

            var bucket = _dbContext.Projects
                .Where(x => x.SubmissionProjectId == submission.Project.Id)
                .Select(x => x.OutputBucketTre);

            var outputBucket = bucket.FirstOrDefault();

            var isFolderExists = _minioTreHelper.FolderExists(outputBucket.ToString(), "sub" + subId).Result;
            if (!isFolderExists)
            {
                var submissionFolder = _minioTreHelper.CreateFolder(outputBucket.ToString(), "sub" + subId).Result;
            }

            outputBucket = outputBucket.ToString();
            return new OutputBucketInfo()
            {
                Bucket = outputBucket,
                SubId = submission.Id.ToString(),
            };
        }


    }
}
