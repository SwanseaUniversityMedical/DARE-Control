using BL.Models;
using BL.Models.Settings;
using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BL.Models.Enums;

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

            connection.On<List<string>>("TREUpdateStatus", UpdateStatusForEndpoint);

            connection.StartAsync();

            return Task.CompletedTask;
        }

        private void UpdateStatusForEndpoint(List<string> varList)
        {
            string endpointname = varList[0];
            string tesId = varList[1];
            string status = varList[2];

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var endpoint = _DbContext.Endpoints.FirstOrDefault(x => x.Name.ToLower() == endpointname.ToLower());
                if (endpoint == null)
                {
                    Log.Error("DAREBackgroundService: Unable to find endpoint");
                    return;
                }

                var sub = _DbContext.Submissions.FirstOrDefault(x => x.TesId == tesId && x.EndPoint == endpoint);
                if (sub == null)
                {
                    Log.Error("DAREBackgroundService: Unable to find submission");
                    return;
                }

                Enum.TryParse(status, out StatusType myStatus);
                UpdateSubmissionStatus.UpdateStatus(sub, myStatus, "");
                sub.Status = myStatus;

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
