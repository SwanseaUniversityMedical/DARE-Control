using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Models.Enums;

namespace BL.Models
{
    public class EgressSubmission
    {
        public int Id { get; set; }
        public string SubmissionId { get; set; }
        public EgressStatus Status { get; set; }
        public string OutputBucket { get; set; }

        public DateTime? Completed { get; set; }
        public string? Reviewer { get; set; }
        public virtual List<EgressFile> Files { get; set; }
    }
}
