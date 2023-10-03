using BL.Models;
using BL.Models.Enums;
using BL.Models.ViewModels;

namespace TRE_API.Services
{
    public interface ISubmissionHelper
    {
        APIReturn? UpdateStatusForTre(string tesId, StatusType statusType, string? description);
        bool IsUserApprovedOnProject(int projectId, int userId);
        List<Submission>? GetWaitingSubmissionForTre();
    }
}
