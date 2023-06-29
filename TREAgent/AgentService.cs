using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Services;
using Microsoft.Extensions.Configuration;

namespace TREAgent
{
    public class AgentService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string TREName;

        public AgentService(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            TREName = configuration["TREName"];
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Perform initialization logic for your hosted service here

           
            using (var scope = _serviceProvider.CreateScope())
            {
                var treApi = scope.ServiceProvider.GetRequiredService<ITREClientHelper>();
                var dareApi = scope.ServiceProvider.GetRequiredService<IDareClientHelper>();

               

                // Start Hangfire background job server
                using (var server = new BackgroundJobServer())
                {
                    // Schedule a recurring job to run every 10 minutes
                    RecurringJob.AddOrUpdate(() => GetSubs(dareApi, treApi), Cron.MinuteInterval(10));

                    // Keep the service running
                    Task.Delay(Timeout.Infinite, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Perform cleanup logic for your hosted service here

            return Task.CompletedTask;
        }

        public void GetSubs(IDareClientHelper dareApi, ITREClientHelper treApi)
        {
            
            

            Console.WriteLine("Job executed!");
        }
    }
}
