using Tre_Camunda.Models;

namespace Tre_Camunda.Services
{
    public interface IServicedZeebeClient
    {
        Task DeployModel(Stream resourceStream, string resourceName);

        Task<DmnResponse> EvaluateDecisionModelAsync(DmnRequest input);

        Task PublishMessageAsync(string messageName, string correlationKey, object variables);

        Task PrintTopologyAsync();
    }

}
    
