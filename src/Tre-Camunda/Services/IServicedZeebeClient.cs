using Zeebe.Client;

namespace Tre_Camunda.Services
{

    public interface IServicedZeebeClient
    {
        Task DeployModel(Stream resourceStream, string resourceName);
    }
  
}
