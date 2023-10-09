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
        public string subId { get; set; }
        public StatusType statusType { get; set; }
        public string? description { get; set; }

        
    }
}
