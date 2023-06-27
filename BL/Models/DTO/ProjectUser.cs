using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BL.Models.DTO
{
    public class ProjectUser
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }

        public IEnumerable<SelectListItem> ProjectItemList { get; set; }

        public IEnumerable<SelectListItem> UserItemList { get; set; }
    }
}
