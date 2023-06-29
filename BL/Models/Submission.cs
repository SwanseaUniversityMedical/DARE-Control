using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string? TesId { get; set; }
        public string SourceCrate { get; set; }

        public string TesName { get; set; }
        public string? TesJson { get; set; }
        public string DockerInputLocation { get; set; }

        //zxczxcxcxcvxc
        
        public virtual Project Project { get; set; }

        [ForeignKey("ParentID")]
        public virtual Submission? Parent { get; set; }

        public virtual List<Submission> Children { get; set; }

        public SubmissionStatus Status { get; set; }

        public virtual Endpoint? EndPoint { get; set; }

        public virtual User SubmittedBy { get; set; }

        public string? StatusDescription { get; set; }
        
        

    }

    public enum SubmissionStatus
    {
        WaitingForChildSubsToComplete =0,
        WaitingForAgentToTransfer=1,
        TransferredToPod=2,
        PodProcessing=3,
        PodProcessingComplete=4,
        DataOutApprovalBegun=5,
        DataOutApprovalRejected=6,
        DataOutApproved=7,
        UserNotOnProject=8,
        InvalidUser=9,
        TRENotAuthorisedForProject=10,
        Completed=11,
        InvalidSubmission=12,
        CancellingChildren = 13,
        RequestCancellation = 14,
        CancellationRequestSent = 15,
        Cancelled = 16,
        WaitingForCrateFormatCheck=17
    }
}
