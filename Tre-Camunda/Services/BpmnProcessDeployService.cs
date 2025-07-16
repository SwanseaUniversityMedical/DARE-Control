using Microsoft.Extensions.Hosting;
using Serilog;
using Tre_Camunda.Settings;

namespace Tre_Camunda.Services
{
    public class BpmnProcessDeployService : IHostedService
    {
        private readonly IBpmnService _bpmnService;
        private readonly IHostEnvironment _env;
        private readonly CamundaSettings _camundaSettings;

        public BpmnProcessDeployService(IBpmnService bpmnService, IHostEnvironment env, CamundaSettings camundaSettings)
        {
            _bpmnService = bpmnService;
            _env = env;
            _camundaSettings = camundaSettings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var mn = Environment.MachineName;
            bool test = _env.IsDevelopment();
            test = true;

            if (test && _camundaSettings.Deploy)
            {
                try
                {
                    await _bpmnService.DeployProcessDefinition();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
