using BL.Models.Enums;

namespace BL.Models
{
    public class TreProject : BaseModel
    {
        public int Id { get; set; }
        public int SubmissionProjectId { get; set; }
        
       
        public string? SubmissionProjectName { get; set; }
        public string? Description { get; set; }
        public virtual List<TreMembershipDecision> MemberDecisions { get; set; }
        public string? LocalProjectName { get; set; }
        public bool Approved { get; set; }
        public bool Archived { get; set; }
        public string? ApprovedBy { get; set; } 
        public DateTime LastDecisionDate { get; set; }
    }
    
}
