using BL.Models.Enums;

namespace BL.Models
{
    public class TreProject : BaseModel
    {
        public int Id { get; set; }
        public int SubmissionProjectId { get; set; }
        
       
        public string? SubmissionProjectName { get; set; }
        public virtual List<TreMembershipDecision> MemberDecisions { get; set; }
        public string? LocalProjectName { get; set; }
        public DecisionStatus Decision { get; set; } 
        public string? ApprovedBy { get; set; } 
        public DateTime Date { get; set; }
    }
    
}
