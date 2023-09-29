

namespace BL.Models.Enums
{
    public enum StatusType
    {
        WaitingForChildSubsToComplete = 0,
        WaitingForAgentToTransfer = 1,
        TransferredToPod = 2,
        PodProcessing = 3,
        PodProcessingComplete = 4,
        DataOutApprovalBegun = 5,
        DataOutApprovalRejected = 6,
        DataOutApproved = 7,
        UserNotOnProject = 8,
        InvalidUser = 9,
        TRENotAuthorisedForProject = 10,
        Completed = 11,
        InvalidSubmission = 12,
        CancellingChildren = 13,
        RequestCancellation = 14,
        CancellationRequestSent = 15,
        Cancelled = 16,
        WaitingForCrateFormatCheck = 17,
        ValidatingUser = 18,
        ValidatingSubmission = 19,
        ValidationSuccessful = 20,
        AgentTransferringToPod = 21,
        TransferToPodFailed = 22,
        TRERejectedProject = 23,
        TREApprovedProject = 24,
        PodProcessingFailed = 25,
        Running = 26, 
        Failed = 27

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
