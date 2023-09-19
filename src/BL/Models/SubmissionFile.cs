using BL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class SubmissionFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TreBucketFullPath { get; set; }
        public string SubmisionBucketFullPath { get; set; }
        public FileStatus Status { get; set; }
        public string Description { get; set; }

        public virtual Submission Submission { get; set; }

    }
}
