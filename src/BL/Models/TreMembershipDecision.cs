using System.Security.AccessControl;
using BL.Models.Enums;

namespace BL.Models
{
    public class TreMembershipDecision : BaseModel
    {
        public int Id { get; set; }
        public virtual TreUser User { get; set; }
        public virtual TreProject Project { get; set; }
        public bool Archived { get; set; }
        public bool Approved { get; set; } 
        public string? ApprovedBy { get; set; } 
        public DateTime LastDecisionDate { get; set; }
    }
    
}
