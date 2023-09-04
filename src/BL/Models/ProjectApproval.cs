namespace BL.Models
{
    public class ProjectApproval : BaseModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        //public virtual List<User> Users { get; set; }
        //public virtual List<Project> Projects { get; set; }
        public string? Projectname { get; set; }
        public string? Username { get; set; }
     
        public string? LocalProjectName { get; set; }
        public string? Approved { get; set; } 
        public string? ApprovedBy { get; set; } 
        public DateTime Date { get; set; }
    }
    public class ProjectApprovalList
    {
        public List<ProjectApproval> ProjectApprovals { get; set; }

    }
}
