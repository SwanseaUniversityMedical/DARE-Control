using Microsoft.Extensions.Hosting;
using Serilog;
using Tre_Camunda.Settings;

namespace Tre_Camunda.Services
{
    public class ProcessDeployService : IHostedService
    {
        private readonly IProcessModelService _processModelService;
        private readonly IHostEnvironment _env;
        private readonly CamundaSettings _camundaSettings;

        public ProcessDeployService(IProcessModelService processModelService, IHostEnvironment env, CamundaSettings camundaSettings)
        {
            _processModelService = processModelService;          
            _camundaSettings = camundaSettings;
            _env = env;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            /* Check to see if used for local development only */
            bool test = _env.IsDevelopment();

            if (test && _camundaSettings.Deploy)
            {
                Log.Information("Starting process and decision model deployment...");
                try
                {
                    await _processModelService.DeployProcessDefinitionAndDecisionModels();
                    Log.Information("Deployment completed successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error("Failed Deployment" + ex.ToString());
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
