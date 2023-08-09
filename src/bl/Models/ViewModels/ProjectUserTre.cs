using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BL.Models.ViewModels
{
    public class ProjectUserTre
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public string? Projectname { get; set; }
        public string? Username { get; set; }
        [JsonIgnore]
        public virtual IEnumerable<SelectListItem>? ProjectItemList { get; set; }
        [JsonIgnore]
        public virtual IEnumerable<SelectListItem>? UserItemList { get; set; }
        public string? LocalProjectName { get; set; } 
    }
}
