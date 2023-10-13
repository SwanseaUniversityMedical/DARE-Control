using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BL.Models.Enums;

namespace BL.Models
{
    public class EgressSubmission
    {
        public int Id { get; set; }
        public string? SubmissionId { get; set; }
        public EgressStatus Status { get; set; }
        public string? OutputBucket { get; set; }

        public string SubFolder { get; set; }

        public DateTime? Completed { get; set; }
        public string? Reviewer { get; set; }
        public virtual List<EgressFile> Files { get; set; }

        public string EgressStatusDisplay
        {
            get
            {
                var enumType = typeof(EgressStatus);
                var memberInfo = enumType.GetMember(Status.ToString());
                var displayAttribute = memberInfo.FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>();

                return displayAttribute?.Name ?? Status.ToString();
            }
        }
    }
}
