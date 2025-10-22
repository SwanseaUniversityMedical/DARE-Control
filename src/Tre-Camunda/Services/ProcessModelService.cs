using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;
using Zeebe.Client;
using Tre_Camunda;

namespace Tre_Camunda.Services
{
    public class ProcessModelService : IProcessModelService
    {
        private IServicedZeebeClient _camunda;
        private readonly IConfiguration _configuration;
     
        public ProcessModelService(IServicedZeebeClient IServicedZeebeClient, IConfiguration configuration)
        {

            _camunda = IServicedZeebeClient;
            _configuration = configuration;           
        }


        
        public async Task DeployProcessDefinitionAndDecisionModels()
        {
            /* Testing connection */
            var gatewayAddress = _configuration["ZeebeBootstrap:Client:GatewayAddress"];
            var modelDirectory = _configuration["ProcessModelSettings:ModelDirectory"];

            //var projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
            var fullModelPath = modelDirectory;

            Console.WriteLine($"Resolved Model Path: {fullModelPath}");
            
            if (string.IsNullOrWhiteSpace(fullModelPath))
            {
                Log.Warning("Model directory not configured in appsettings.json");
                return;
            }            

            var zeebeClient = ZeebeClient.Builder()
                .UseGatewayAddress(gatewayAddress)
                .UsePlainText()
                .Build();

            var topology = await zeebeClient.TopologyRequest().Send();
            Console.WriteLine($"Connected to cluster with version");


            var modelFiles = Directory.GetFiles(fullModelPath, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".bpmn", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase))
            .ToList();


            if (!modelFiles.Any())
            {
                Log.Warning("No BPMN or DMN models found to deploy.");
                return;
            }

            foreach (var model in modelFiles)
            {
                var fileName = Path.GetFileName(model);
                Log.Information($"Deploying process definition: {fileName}");

                try
                {
                    using var stream = File.OpenRead(model);
                    await _camunda.DeployModel(stream, fileName);
                    Log.Information($"Successfully deployed: {fileName}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to deploy {fileName}: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
