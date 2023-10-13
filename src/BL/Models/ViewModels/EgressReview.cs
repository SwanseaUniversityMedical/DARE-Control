using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class EgressReview
    {
        public string subId { get; set; }
        public List<EgressResult> fileResults { get; set; }
    }

    public class EgressResult
    {
        public string? fileName { get; set; }
        public bool approved { get; set; } 
    }
}
