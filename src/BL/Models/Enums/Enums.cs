

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
        Failed = 27,
        //Remove these later
        SendingFileToHUTCH = 28,
        SendingToHUTCHForFinalPackaging = 29,

        //Internal Hutch Statuses
        /// <summary>
        /// Waiting for a Crate payload or Crate URL to be submitted to Hutch.
        /// </summary>
        [Display(Name = "Waiting for a Crate")]
        WaitingForCrate = 30,

        /// <summary>
        /// A Crate has been submitted via URL; Hutch is fetching it.
        /// </summary>
        [Display(Name = "Fetching Crate")]
        FetchingCrate = 31,

        /// <summary>
        /// Hutch has queued the Crate to be executed.
        /// </summary>
        [Display(Name = "Create queued")]
        Queued = 32,

        /// <summary>
        /// Hutch is validating the Job Crate metadata prior to execution.
        /// </summary>
        [Display(Name = "Validating Crate")]
        ValidatingCrate = 33,

        /// <summary>
        /// Hutch is fetching the remote workflow referenced by the crate.
        /// </summary>
        [Display(Name = "Fetching workflow")]
        FetchingWorkflow=34,

        /// <summary>
        /// Hutch is preparing the workflow for the Workflow Executor (e.g. WfExS)
        /// using a combination of the Job Crate, the Workflow Crate and Executor configuration.
        /// </summary>
        [Display(Name = "Preparing workflow")]
        StagingWorkflow=35,

        /// <summary>
        /// Hutch has triggered the Workflow Executor to run the Worflow.
        /// </summary>
        [Display(Name = "Executing workflow")]
        ExecutingWorkflow = 36,

        /// <summary>
        /// Following successful Workflow Execution, Hutch is preparing outputs
        /// for an Egress request (e.g. disclosure checks etc.)
        /// </summary>
        [Display(Name = "Preparing outputs")]
        PreparingOutputs = 37,

        /// <summary>
        /// Hutch has requested Egress and/or is providing outputs for Egress checking
        /// </summary>
        [Display(Name = "Requested Egress")]
        DataOutRequested=38,

        /// <summary>
        /// Hutch has provided outputs for Egress and is awaiting the results of checks
        /// </summary>
        [Display(Name = "Waiting for Egress results")]
        TransferredForDataOut = 39,

        /// <summary>
        /// Egress checks have approved some or all outputs; Hutch is finalising the approved results RO-Crate package.
        /// </summary>
        [Display(Name = "Finalising approved results")]
        PackagingApprovedResults=40,

        /// <summary>
        /// Hutch has transferred the results RO-Crate package to the Intermediary Store.
        /// </summary>
        [Display(Name = "Completed")]
        Complete=41,

        /// <summary>
        /// Hutch was unable to get the job to a complete status; it failed during the previous status stage.
        /// </summary>
        [Display(Name = "Failed")]
        Failure = 42

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
