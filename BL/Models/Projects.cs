using Newtonsoft.Json;

namespace BL.Models
{
    public class Projects
    {
        public int Id { get; set; }
        [JsonIgnore]
        public List<User> Users { get; set; }

        public List<Endpoints> Endpoints { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual List<ProjectMembership> ProjectMemberships { get; set; }

        public virtual List<Submission> Submissions { get; set; }

    }



    public class ProjectListModel
    {
        public List<Projects> Projects { get; set; }
    }
}
