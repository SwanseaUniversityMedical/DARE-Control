using BL.Models;
using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DARE_API.Services
{
    public class DAREBackgroundService : IHostedService, IDisposable
    {
        private Timer _timer;
        HubConnection connection;
        private readonly TREAPISettings _APISettings;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DAREBackgroundService(IOptions<TREAPISettings> APISettings, IServiceScopeFactory serviceScopeFactory)
        {
            _APISettings = APISettings.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            connection = new HubConnectionBuilder()
    .WithUrl(_APISettings.SignalRAddress)
    .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<string, string, SubmissionStatus>("TREMessage", UpdateStatusForEndpoint);

            connection.StartAsync();

            return Task.CompletedTask;
        }

        private void UpdateStatusForEndpoint(string endpointname, string tesId, SubmissionStatus status)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var endpoint = _DbContext.Endpoints.FirstOrDefault(x => x.Name.ToLower() == endpointname.ToLower());
                if (endpoint == null)
                {
                    //return BadRequest("No access to endpoint " + endpointname + " or does not exist.");
                }

                var sub = _DbContext.Submissions.FirstOrDefault(x => x.TesId == tesId && x.EndPoint == endpoint);
                if (sub == null)
                {
                    //return BadRequest("Invalid tesid or endpoint not valid for tes");
                }
                sub.Status = status;

                _DbContext.SaveChanges();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
