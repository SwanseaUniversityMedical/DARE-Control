
namespace BL.Models
{
    public class Endpoint
    {
        public int Id { get; set; }        
        public virtual List<Project> Projects { get; set; }
        public string Name { get; set; }
        public string FormData { get; set; }
        public virtual List<Submission> Submissions { get; set; }

    }
}
