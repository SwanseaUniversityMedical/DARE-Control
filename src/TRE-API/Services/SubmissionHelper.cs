using BL.Models;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Services;
using EasyNetQ;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services.SignalR;

namespace TRE_API.Services
{
    public class SubmissionHelper: ISubmissionHelper
    {
        private readonly IHutchClientHelper _hutchHelper;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IBus _rabbit;


        public SubmissionHelper(ISignalRService signalRService, IDareClientWithoutTokenHelper helper,
            ApplicationDbContext dbContext, IBus rabbit, IHutchClientHelper hutchHelper)
        {
            
            _dareHelper = helper;
            _dbContext = dbContext;
            _rabbit = rabbit;
            _hutchHelper = hutchHelper;
        }

        public void SendSumissionToHUTCH(Dictionary<string, string> SubmissionData)
        {
            var statusParams = new Dictionary<string, string>()
            {
                { "tesId", SubmissionData["SubmissionId"] },
                { "statusType", StatusType.SendingFileToHUTCH.ToString() },
                { "description", "" }
            };
            var StatusResult = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre", statusParams);

            var res = _hutchHelper.CallAPIWithoutModel<APIReturn>($"/api/jobs/{SubmissionData["SubmissionId"]}",
                SubmissionData); //Need to update this when parameters are known
        }

        public APIReturn? UpdateStatusForTre(string tesId, StatusType statusType, string? description)
        {
            var result = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                    new Dictionary<string, string>()
                        { { "tesId", tesId }, { "statusType", statusType.ToString() }, { "description", description } })
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
            var result =
                _dareHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForTre").Result;
            return result;
        }
    }
}
