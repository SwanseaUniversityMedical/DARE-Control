using BL.Models.Enums;

namespace BL.Models
{
    public class TreUser : BaseModel
    {
        public int Id { get; set; }
        public int SubmissionUserId { get; set; }

        public virtual List<TreMembershipDecision> MemberDecisions { get; set; }

        public bool Archived { get; set; }

        public string? Username { get; set; }
        public string? Email { get; set; }



    }
    
}
