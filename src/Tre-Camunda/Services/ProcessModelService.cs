﻿using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;
using Zeebe.Client;
using Tre_Camunda;

namespace Tre_Camunda.Services
{
    public class ProcessModelService : IProcessModelService
    {
        private IServicedZeebeClient _camunda;

        public ProcessModelService(IServicedZeebeClient IServicedZeebeClient)
        {

            _camunda = IServicedZeebeClient;
        }

       

        public async Task DeployProcessDefinitionAndDecisionModels()
        {
            /* Testing connection */
            var zeebeClient = ZeebeClient.Builder()
                .UseGatewayAddress("localhost:26500")
                .UsePlainText()
                .Build();

            var topology = await zeebeClient.TopologyRequest().Send();
            Console.WriteLine($"Connected to cluster with version");


            var modelResources = Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(x => x.ToLower().Contains(".processmodels.") &&
            (x.EndsWith(".bpmn", StringComparison.OrdinalIgnoreCase) ||
            x.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase)))
            .ToList();

            if (!modelResources.Any())
            {
                Log.Warning("No BPMN or DMN models found to deploy.");
                return;
            }

            foreach (var model in modelResources)
            {
                var fileExtension = Path.GetExtension(model);
                var name = Path.GetFileNameWithoutExtension(model.Split('.').Last());
                var deploymentFileName = $"{name}{fileExtension}";
                Log.Information($"Deploying process definition with name: {deploymentFileName}");

                using var resourceStream = GetType().Assembly.GetManifestResourceStream(model);

                if (resourceStream == null)
                {
                    Log.Warning("Could not find resource stream for: " + model);
                    continue;
                }

                try
                {
                    await _camunda.DeployModel(resourceStream, deploymentFileName);
                    Log.Information($"Successfully deployed: {deploymentFileName}");
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to deploy process definition with name: {deploymentFileName}, error: {e}");
                    throw new ApplicationException("Failed to deploy process definition", e);
                }
            }
        }
    }
}
