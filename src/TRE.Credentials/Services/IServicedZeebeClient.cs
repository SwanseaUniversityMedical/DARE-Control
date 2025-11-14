using System.IO;
using System.Threading.Tasks;
using Tre_Credentials.Models.Zeebe;
using Zeebe.Client;

namespace Tre_Credentials.Services
{
    public interface IServicedZeebeClient
    {
        Task DeployModel(Stream resourceStream, string resourceName);

        Task<DmnResponse> EvaluateDecisionModelAsync(DmnRequest input);

        Task PublishMessageAsync(string messageName, string correlationKey, object variables);

        Task PrintTopologyAsync();
    }
}
