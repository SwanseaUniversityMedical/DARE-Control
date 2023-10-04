

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
        SendingFileToHUTCH = 18,
        SendingToHUTCHForFinalPackaging = 19
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
