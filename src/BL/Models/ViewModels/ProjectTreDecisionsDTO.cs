using BL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class ProjectTreDecisionsDTO
    {
        public int ProjectId { get; set; }
        public Decision Decision { get; set; }
    }
}
