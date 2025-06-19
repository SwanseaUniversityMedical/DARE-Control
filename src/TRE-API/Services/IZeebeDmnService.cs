using TRE_API.Models;

namespace TRE_API.Services
{
    public interface IZeebeDmnService
    {
        Task<DmnResponse> EvaluateDecisionModelAsync(DmnRequest input);        
    }
}
