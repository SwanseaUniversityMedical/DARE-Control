using System.ComponentModel.DataAnnotations;

namespace BL.Models.Enums
{
    public enum StatusType
    {
        //Stage 1
        //Parent only
        [Display(Name = "Waiting for Child Subs To Complete")]
        WaitingForChildSubsToComplete = 0,

        //Stage 1
        [Display(Name = "Waiting for Agent To Transfer")]
        WaitingForAgentToTransfer = 1,

        //Stage 2
        [Display(Name = "Transferred To Agent")]
        TransferredToAgent = 2,

        //Stage 3
        [Display(Name = "Processing Submission")]
        Processing = 3,

        //Stage 3
        //Green
        [Display(Name = "Processing Complete")]
        ProcessingComplete = 4,

        //Stage 4
        [Display(Name = "Data Out Approval Begun")]
        DataOutApprovalBegun = 5,

        //Stage 4
        //Red
        [Display(Name = "Data Out Rejected")] DataOutApprovalRejected = 6,

        //Stage 4
        //Green
        [Display(Name = "Data Out Approved")] DataOutApproved = 7,

        //Stage 1
        //Red
        [Display(Name = "User Not On Project")]
        UserNotOnProject = 8,

        //Stage 2
        //Red
        [Display(Name = "User not authorised for project on TRE")]
        InvalidUser = 9,

        //Stage 2
        //Red
        [Display(Name = "TRE Not Authorised For Project")]
        TreNotAuthorisedForProject = 10,

        //Stage 5
        //Green
        [Display(Name = "Completed")] Completed = 11,

        //Stage 1
        //Red
        [Display(Name = "Invalid Submission")] InvalidSubmission = 12,

        //Stage 1
        //Red
        [Display(Name = "Cancelling Children")]
        CancellingChildren = 13,

        //Stage 1
        //Red
        [Display(Name = "Request Cancellation")]
        RequestCancellation = 14,

        //Stage 1
        //Red
        [Display(Name = "Cancellation Request Sent")]
        CancellationRequestSent = 15,

        //Stage 5
        //Red
        [Display(Name = "Cancelled")] Cancelled = 16,

        //Unused
        [Display(Name = "Validating User")] ValidatingUser = 18,

        //Unused
        [Display(Name = "Validating Submission")]
        ValidatingSubmission = 19,

        //Unused
        //Green
        [Display(Name = "Validation Successful")]
        ValidationSuccessful = 20,

        //Stage 2
        [Display(Name = "Agent Transferring To Pod")]
        AgentTransferringToPod = 21,

        //Stage 2
        //Red
        [Display(Name = "Transfer To Pod Failed")]
        TransferToPodFailed = 22,

        //Unused
        [Display(Name = "Tre Rejected Project")]
        TreAgentRejectedProject = 23,

        //Unused
        [Display(Name = "Tre Approved Project")]
        TreApprovedProject = 24,

        //Stage 3
        //Red
        [Display(Name = "Processing Failed")] ProcessingFailed = 25,

        //Stage 1
        //Parent only
        [Display(Name = "Running")] Running = 26,

        //Stage 5
        //Red
        [Display(Name = "Failed")]
        Failed = 27,

        //Stage 3
        [Display(Name = "Fetching workflow")] FetchingWorkflow = 34,

        //Stage 3
        [Display(Name = "Preparing workflow")] StagingWorkflow = 35,

        //Stage 3
        [Display(Name = "Executing workflow")] ExecutingWorkflow = 36,

        //Stage 3
        [Display(Name = "Preparing outputs")] PreparingOutputs = 37,

        //Stage 3
        [Display(Name = "Requested Egress")] DataOutRequested = 38,

        //Stage 3
        [Display(Name = "Waiting for Egress results")]
        TransferredForDataOut = 39,

        //Stage 3
        [Display(Name = "Finalising approved results")]
        PackagingApprovedResults = 40,

        //Stage 3
        //Green
        [Display(Name = "Completed")] Complete = 41,

        //Stage 3
        //Red
        [Display(Name = "Failed")] Failure = 42,

        //Stage 1
        [Display(Name = "Submission has been received")]
        SubmissionReceived = 43,

        //Stage 5
        //Green
        //Parent Only
        [Display(Name = "Complete but not all TREs returned a result")]
        PartialResult = 49,
    }

    public enum Decision
    {
        Undecided = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum FileStatus
    {
        Undecided = 0,
        Approved = 1,
        Rejected = 2
    }


    public enum EgressStatus
    {
        [Display(Name = "Not completed")] NotCompleted = 0,
        [Display(Name = "Fully Approved")] FullyApproved = 1,
        [Display(Name = "Fully Rejected")] FullyRejected = 2,
        [Display(Name = "Partially Approved")] PartiallyApproved = 3
    }
}