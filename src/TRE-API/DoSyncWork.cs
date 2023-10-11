using Microsoft.Extensions.Configuration;
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Models.Tes;
using BL.Rabbit;
using BL.Services;
using Microsoft.Extensions.DependencyInjection;
using EasyNetQ;
using Newtonsoft.Json;
using TRE_API.Services;


namespace TRE_API
{
    public interface IDoSyncWork
    {
        void Execute();
    }

    public class DoSyncWork : IDoSyncWork
    {
        private readonly IServiceProvider _serviceProvider;

        public DoSyncWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        
        }

        public void Execute()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dareSyncHelper = scope.ServiceProvider.GetRequiredService<IDareSyncHelper>();
                var result = dareSyncHelper.SyncSubmissionWithTre().Result;
                
            }
        }
    }
}