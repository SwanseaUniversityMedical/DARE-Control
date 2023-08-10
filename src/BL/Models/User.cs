namespace BL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public virtual List<Project> Projects { get; set; }
        public virtual List<Submission> Submissions { get; set; }
        public string FormData { get; set; }
    }
}
