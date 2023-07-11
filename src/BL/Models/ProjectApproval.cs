using BL.Models.DTO;
using Newtonsoft.Json;

namespace BL.Models
{
    public class ProjectApproval
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public virtual List<Project> Projects { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ProjectName { get; set; }
        public string? Approved { get; set; }
        public string? SubmittedBy { get; set; }

    }
}
