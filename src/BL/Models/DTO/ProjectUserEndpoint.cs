using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BL.Models.DTO
{
    public class ProjectUserEndpoint
    {
        public int Id { get; set; }
        public virtual List<User> Users { get; set; }
        public virtual List<Endpoint> Endpoints { get; set; }
        public string FormData { get; set; }
        public string FormIoUrl { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SubmissionBucket { get; set; }
        public string? OutputBucket { get; set; }
        public string? MinioEndpoint { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? EndpointItemList { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? UserItemList { get; set; }
    }
}
