using Zeebe.Client;



namespace Tre_Camunda.Services
{
    public interface IServicedZeebeClient
    {
        Task DeployModel(Stream resourceStream, string resourceName);
    }


    public class ServicedZeebeClient : IServicedZeebeClient
    {
        public IZeebeClient _IZeebeClient;
        public IServiceProvider _serviceProvider;

        public ServicedZeebeClient(IZeebeClient IZeebeClient, IServiceProvider serviceProvider)
        {
            _IZeebeClient = IZeebeClient;
            _serviceProvider = serviceProvider;
            // _clientHelper = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IClientHelper>();
        }



        public async Task DeployModel(Stream resourceStream, string resourceName)
        {
            var deployResponse = await _IZeebeClient.NewDeployCommand()
                    .AddResourceStream(resourceStream, resourceName)
                    .Send();

            Console.WriteLine($"Deployed: {string.Join(", ", deployResponse.Processes.Select(p => p.BpmnProcessId))}");

        }

        public async Task PublishMessageAsync(string messageName, string correlationKey, object variables)
        {
            var variablesJson = System.Text.Json.JsonSerializer.Serialize(variables);

            await _IZeebeClient.NewPublishMessageCommand()
                .MessageName(messageName)
                .CorrelationKey(correlationKey)
                .Variables(variablesJson)
                .Send();

            Console.WriteLine($"Published message: {messageName} with correlation key: {correlationKey}");
        }

        private static IServiceProvider serviceProvider;

        public async Task PrintTopologyAsync()
        {
            var topology = await _IZeebeClient.TopologyRequest().Send();
            Console.WriteLine(topology);
        }


    }
}
