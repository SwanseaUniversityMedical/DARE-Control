using Microsoft.Extensions.Configuration;
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using BL.Models.Tes;
using BL.Rabbit;
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

        


        public DoWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        
        }

        public void Execute()
        {

           
            using (var scope = _serviceProvider.CreateScope())
            {
                var rabbit = scope.ServiceProvider.GetRequiredService<IBus>(); ;
                var exch = rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Main, "topic");
                var treApi = scope.ServiceProvider.GetRequiredService<ITreClientWithoutTokenHelper>();
                
                
                var subs = treApi.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForTre").Result;


               
                
                foreach (var submission in subs)
                {
                    var paramlist = new Dictionary<string, string>
                    {
                        { "projectId", submission.Project.Id.ToString() },
                        { "userId", submission.SubmittedBy.Id.ToString() }

                    };

                    if (!treApi.CallAPIWithoutModel<BoolReturn>("/api/Submission/IsUserApprovedOnProject", paramlist)
                            .Result.Result)
                    {
                        var result = treApi.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                            new Dictionary<string, string>()
                            {
                                { "tesId", submission.TesId }, { "statusType", StatusType.InvalidUser.ToString() },
                                { "description", "" }
                            }).Result;
                    }
                    else
                    {


                        //TODO: Check crate format
                        //TODO: Call API or rabbit for testing (Only dump tesString)

                        var tes = JsonConvert.DeserializeObject<TesTask>(submission.TesJson);
                        rabbit.Advanced.Publish(exch, RoutingConstants.Subs, false, new Message<TesTask>(tes));
                        var result = treApi.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                            new Dictionary<string, string>()
                            {
                                { "tesId", submission.TesId }, { "statusType", StatusType.TransferredToPod.ToString() },
                                { "description", "" }
                            }).Result;
                        
                    }

                }


            }
            // Use the app settings value here

        }
    }
}