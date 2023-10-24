using BL.Models;
using BL.Models.Enums;
using BL.Models.ViewModels;
using static TRE_API.Controllers.SubmissionController;

namespace TRE_API.Services
{
    public interface ISubmissionHelper
    {
        APIReturn? UpdateStatusForTre(string subId, StatusType statusType, string? description);
        bool IsUserApprovedOnProject(int projectId, int userId);
        List<Submission>? GetWaitingSubmissionForTre();
        void SendSumissionToHUTCH(Submission submission);
        List<Submission>? GetRequestCancelSubsForTre();
        OutputBucketInfo GetOutputBucketGuts(string subId);
        APIReturn? CloseSubmissionForTre(string subId, StatusType statusType, string? description, string? finalFile);
    }
}
