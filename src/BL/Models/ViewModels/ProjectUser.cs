using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BL.Models.ViewModels
{
    public class ProjectUser
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        [JsonIgnore]
        public IEnumerable<SelectListItem>? ProjectItemList { get; set; }
        [JsonIgnore]
        public IEnumerable<SelectListItem>? UserItemList { get; set; }
    }
}
