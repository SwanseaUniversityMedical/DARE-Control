using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.DTO
{
    public class ProjectEndpoint
    {
        public int ProjectId { get; set; }
        public int EndpointId { get; set; }

        public IEnumerable<SelectListItem> ProjectItemList { get; set; }

        public IEnumerable<SelectListItem> EndpointItemList { get; set; }
    }
}
