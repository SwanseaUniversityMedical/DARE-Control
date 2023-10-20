using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Models.Enums;

namespace BL.Models.ViewModels
{
    public class SubmissionDetails
    {
        public string SubId { get; set; }
        public StatusType StatusType { get; set; }
        public string? Description { get; set; }

        
    }
}
