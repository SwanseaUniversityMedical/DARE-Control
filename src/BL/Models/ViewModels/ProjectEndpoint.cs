using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BL.Models.ViewModels
{
    public class ProjectEndpoint
    {
        public int ProjectId { get; set; }
        public int EndpointId { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? ProjectItemList { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? EndpointItemList { get; set; }
    }
}
