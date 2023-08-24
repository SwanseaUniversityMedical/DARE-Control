using Microsoft.Extensions.Configuration;
using BL.Models;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Models.Tes;
using BL.Rabbit;
using BL.Services;
using Microsoft.Extensions.DependencyInjection;
using EasyNetQ;
using Newtonsoft.Json;
using TREAgent.Services;

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
                var treApi = scope.ServiceProvider.GetRequiredService<ITreClientWithoutTokenHelper>();
                
                
                var subs = treApi.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForEndpoint").Result;


               
                
                foreach (var submission in subs)
                {
                    //TODO: Validate against treapi
                    //TODO: Check crate format
                    //TODO: Call API or rabbit for testing (Only dump tesString)

                    var tes = JsonConvert.DeserializeObject<TesTask>(submission.TesJson);
                    rabbit.Advanced.Publish(exch, RoutingConstants.Subs, false, new Message<TesTask>(tes));
                    var result = treApi.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForEndpoint",
                        new Dictionary<string, string>() {  {"tesId", submission.TesId}, {"statusType",StatusType.TransferredToPod.ToString() }, {"description", "" }}).Result;
                    //TODO: Update statusType of subs

                }


            }
            // Use the app settings value here

        }
    }
}