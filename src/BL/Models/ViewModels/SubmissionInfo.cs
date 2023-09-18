using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class SubmissionInfo
    {
        
        public Submission Submission { get; set; }
        public List<StageInfo> StageInfo { get; set; }
    }
}
