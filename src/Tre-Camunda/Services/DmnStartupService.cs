using Tre_Camunda.Models;

namespace Tre_Camunda.Services
{
    public class DmnStartupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;       
        public DmnStartupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;           
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            //var dmnService = scope.ServiceProvider.GetRequiredService<ZeebeDmnService>();
            var zeebeDmnService = scope.ServiceProvider.GetRequiredService<IZeebeDmnService>();
            var result = await zeebeDmnService.EvaluateDecisionModelAsync(new DmnRequest
            {
                DecisionId = "Decision_07jt7ht",
                Variables = new Dictionary<string, object>
            {
                { "name", "Gayathri Menon" },
                { "timeOfDay", "morning" }
            }
            });

            Console.WriteLine("DMN Evaluation Result at Startup");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

