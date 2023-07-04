using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Models;
using BL.Models.DTO;
using BL.Models.Tes;
using BL.Rabbit;
using BL.Services;
using Microsoft.Extensions.DependencyInjection;
using EasyNetQ;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

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
                var rabbit = scope.ServiceProvider.GetRequiredService<IBus>(); ;
                var exch = rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Main, "topic");
                var treApi = scope.ServiceProvider.GetRequiredService<ITREClientHelper>();
                var dareApi = scope.ServiceProvider.GetRequiredService<IDareClientHelper>();
                
        var subs = dareApi.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForEndpoint",
                    new Dictionary<string, string>() { { "endpointname", TreName } }).Result;


               
                
                foreach (var submission in subs)
                {
                    //TODO: Validate against treapi
                    //TODO: Check crate format
                    //TODO: Call API or rabbit for testing (Only dump tesString)

                    var tes = JsonConvert.DeserializeObject<TesTask>(submission.TesJson);
                    rabbit.Advanced.Publish(exch, RoutingConstants.Subs, false, new Message<TesTask>(tes));
                    var result = dareApi.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForEndpoint",
                        new Dictionary<string, string>() { { "endpointname", TreName}, {"tesId", submission.TesId}, {"status",SubmissionStatus.TransferredToPod.ToString() } }).Result;
                    //TODO: Update status of subs

                }


            }
            // Use the app settings value here

        }
    }
}