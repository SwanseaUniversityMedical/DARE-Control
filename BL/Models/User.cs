using Newtonsoft.Json;

namespace BL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public virtual List<ProjectMembership> ProjectMemberships { get; set; }
        public virtual List<Submission> Submissions { get; set; }
        public virtual FormData FormData { get; set; }
    }
}
