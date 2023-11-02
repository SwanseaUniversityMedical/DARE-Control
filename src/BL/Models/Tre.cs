
namespace BL.Models
{
    public class Tre: BaseModel
    {
        public int Id { get; set; }        
        public virtual List<Project> Projects { get; set; }
        public string Name { get; set; }

        public DateTime LastHeartBeatReceived { get; set; }
        public string AdminUsername { get; set; }

        public string About {  get; set; }
        public string FormData { get; set; }
        public virtual List<Submission> Submissions { get; set; }

        public virtual List<ProjectTreDecision> ProjectTreDecisions { get; set; }

        public virtual List<MembershipTreDecision> MembershipTreDecision { get; set; }
        public virtual List<AuditLog> AuditLogs { get; set; }

    }
}
