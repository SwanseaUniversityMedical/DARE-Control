

using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BL.Models.Enums
{
    public enum StatusType
    {
        [Display(Name = "Waiting for Child Subs To Complete")]
        WaitingForChildSubsToComplete = 0,
        [Display(Name = "Waiting for Agent To Transfer")]
        WaitingForAgentToTransfer = 1,
        [Display(Name = "Transferred To Pod")]
        TransferredToPod = 2,
        [Display(Name = "Pod Processing")]
        PodProcessing = 3,
        [Display(Name = "Pod Processing Complete")]
        PodProcessingComplete = 4,
        [Display(Name = "Data Out Approval Begun")]
        DataOutApprovalBegun = 5,
        [Display(Name = "Data Out Rejected")]
        DataOutApprovalRejected = 6,
        [Display(Name = "Data Out Approved")]
        DataOutApproved = 7,
        [Display(Name = "User Not On Project")]
        UserNotOnProject = 8,
        [Display(Name = "Invalid User")]
        InvalidUser = 9,
        [Display(Name = "TRE Not Authorised For Project")]
        TRENotAuthorisedForProject = 10,
        [Display(Name = "Completed")]
        Completed = 11,
        [Display(Name = "Invalid Submission")]
        InvalidSubmission = 12,
        [Display(Name = "Cancelling Children")]
        CancellingChildren = 13,
        [Display(Name = "Request Cancellation")]
        RequestCancellation = 14,
        [Display(Name = "Cancellation Request Sent")]
        CancellationRequestSent = 15,
        [Display(Name = "Cancelled")]
        Cancelled = 16,
        [Display(Name = "Waiting For Crate Format Check")]
        WaitingForCrateFormatCheck = 17,
        [Display(Name = "Validating User")]
        ValidatingUser = 18,
        [Display(Name = "Validating Submission")]
        ValidatingSubmission = 19,
        [Display(Name = "Validation Successful")]
        ValidationSuccessful = 20,
        [Display(Name = "Agent Transferring To Pod")]
        AgentTransferringToPod = 21,
        [Display(Name = "Transfer To Pod Failed")]
        TransferToPodFailed = 22,
        [Display(Name = "Tre Rejected Project")]
        TRERejectedProject = 23,
        [Display(Name = "Tre Approved Project")]
        TREApprovedProject = 24,
        [Display(Name = "Pod Processing Failed")]
        PodProcessingFailed = 25,
        [Display(Name = "Running")]
        Running = 26,
        [Display(Name = "Failed")]
        Failed = 27
            //Remove these later
            SendingFileToHUTCH = 28,
        SendingToHUTCHForFinalPackaging = 29
    }

    public enum Decision
    {
        Undecided = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum FileStatus
    {
        ReadyToProcess = 0,
        Approved = 1,
        Rejected = 2
    }


}
