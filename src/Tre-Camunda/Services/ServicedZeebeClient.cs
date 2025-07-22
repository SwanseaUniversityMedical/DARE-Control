using Zeebe.Client;

namespace Tre_Camunda.Services
{
    public class ServicedZeebeClient : IServicedZeebeClient
    {
        public IZeebeClient _IZeebeClient;
        public IServiceProvider _serviceProvider;

        public ServicedZeebeClient(IZeebeClient IZeebeClient, IServiceProvider serviceProvider)
        {
            _IZeebeClient = IZeebeClient;
            _serviceProvider = serviceProvider;           
        }



        public async Task DeployModel(Stream resourceStream, string resourceName)
        {
            var deployResponse = await _IZeebeClient.NewDeployCommand()
                    .AddResourceStream(resourceStream, resourceName)
                    .Send();

        }


    }
}
