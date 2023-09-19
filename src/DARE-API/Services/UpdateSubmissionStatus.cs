using BL.Models.Enums;
using BL.Models;


namespace DARE_API.Services
{
    public class UpdateSubmissionStatus
    {
        public static void UpdateStatus(Submission sub, StatusType type, string? description)
        {
            sub.HistoricStatuses.Add(new HistoricStatus()
            {
                Start = sub.LastStatusUpdate.ToUniversalTime(),
                End = DateTime.Now.ToUniversalTime(),
                Status = sub.Status,
                StatusDescription = sub.StatusDescription
            });
            if (type == StatusType.Cancelled || type == StatusType.Completed)
            {
                sub.EndTime = DateTime.Now.ToUniversalTime();
            }
            sub.Status = type;
            sub.LastStatusUpdate = DateTime.Now.ToUniversalTime();
            sub.StatusDescription = description;
            
            
  
        }
    }
}
