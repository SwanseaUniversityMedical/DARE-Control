using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TREAgent
{
    public interface IDoWork
    {
        void Execute();
    }

    public class DoWork : IDoWork
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly string TreName;


        public DoWork(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            TreName = configuration["TREName"];
        }

        public void Execute()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var treApi = scope.ServiceProvider.GetRequiredService<ITREClientHelper>();
                var dareApi = scope.ServiceProvider.GetRequiredService<IDareClientHelper>();


            }
            // Use the app settings value here

        }
    }
}