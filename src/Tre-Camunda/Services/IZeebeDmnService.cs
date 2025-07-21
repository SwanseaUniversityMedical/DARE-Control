using static Google.Apis.Requests.BatchRequest;
using Tre_Camunda.Models;

namespace Tre_Camunda.Services
{
    public interface IZeebeDmnService
    {
        Task<DmnResponse> EvaluateDecisionModelAsync(DmnRequest input);
    }
}
