
namespace BL.Models
{
    public class Project : BaseModel
    {
        public int Id { get; set; }
        
        public virtual List<User> Users { get; set; }

        public virtual List<Tre> Tres { get; set; }
        public string FormData { get; set; }
        public string Name { get; set; }
        public string Display { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsEmbargoed { get; set; }

        public string ProjectDescription { get; set; }
        public string? SubmissionBucket { get; set; }
        public string? OutputBucket { get; set; }

        
        public virtual List<Submission> Submissions { get; set; }

    }



    public class ProjectListModel
    {
        public List<Project> Projects { get; set; }

    }
}
