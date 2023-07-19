using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DARE_API.Services
{
    public class BackgroundService : IHostedService, IDisposable
    {
        private Timer _timer;

        public BackgroundService()
        {
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            // Your background task logic goes here.
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
