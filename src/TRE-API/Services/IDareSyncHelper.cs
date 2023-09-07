using BL.Models.APISimpleTypeReturns;

namespace TRE_API.Services
{
    public interface IDareSyncHelper
    {
        Task<BoolReturn> SyncSubmissionWithTre();
    }
}
