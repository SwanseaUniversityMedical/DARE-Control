using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;


namespace BL.Models.ViewModels
{
    public class ProjectUserTre
    {
        public int Id { get; set; }
        public virtual List<User> Users { get; set; }
        public virtual List<Tre> Tres { get; set; }
        public string FormData { get; set; }
        public string FormIoUrl { get; set; }
        public string Name { get; set; }

        public string ProjectDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SubmissionBucket { get; set; }
        public string? OutputBucket { get; set; }
        public string? MinioEndpoint { get; set; }

        [JsonIgnore]
        public virtual List<Submission> Submissions { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? TreItemList { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? UserItemList { get; set; }
    }
}
