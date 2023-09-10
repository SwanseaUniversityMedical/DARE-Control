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
using Serilog;
using TREAgent.Services;
using Microsoft.Extensions.Hosting;
using Hangfire;

namespace TREAgent
{
    public interface IDoWork
    {
        void Execute();
        void CheckTESK(string taskID);
    }

    public class DoWork : IDoWork
    {
        private readonly IServiceProvider _serviceProvider;
        public DoWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        
        }

        public void CheckTESK(string taskID)
        {
            Console.WriteLine(taskID);
            RecurringJob.RemoveIfExists("task-999");
        }


        // Method executed upon hangfire job
        public void Execute()
        {
            // control use of dependency injection
            using (var scope = _serviceProvider.CreateScope())
            {

                RecurringJob.AddOrUpdate<IDoWork>("task-999",a => a.CheckTESK("simon"), Cron.MinuteInterval(1));

                // OPTIONS
                // TODO get these from somewhere

                var useRabbit = true;
                var useHutch = true;
                var useTESK = true;

                // Get list of submissions
                List<Submission> listOfSubmissions;
                var treApi = scope.ServiceProvider.GetRequiredService<ITreClientWithoutTokenHelper>();
                try
                {
                    listOfSubmissions = treApi.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForTre").Result;
                }
                catch (Exception e)
                {
                   Log.Error("Error getting submissions: {message}", e.Message);
                   Console.WriteLine(e.Message);
                   throw;
                }
               
                foreach (var aSubmission in listOfSubmissions)
                {
                    Log.Information("Submission: {submission}", aSubmission);

                    var paramlist = new Dictionary<string, string>();
                    try
                    {
                        paramlist.Add("projectId", aSubmission.Project.Id.ToString());
                        paramlist.Add("userId", aSubmission.SubmittedBy.Id.ToString());
                    }
                    catch (Exception e)
                    {
                        Log.Error("Submission does not have project and/or user");
                    }
                    
                    // Check user is allowed ont he project
                    if ( paramlist.Count==2 && !treApi.CallAPIWithoutModel<BoolReturn>("/api/Submission/IsUserApprovedOnProject", paramlist).Result.Result)
                    {
                        Log.Error("User/project {details} is not value for this submission {submission}", paramlist, aSubmission);
                        // record error with submission layer
                        var result = treApi.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                            new Dictionary<string, string>()
                            {
                                { "tesId", aSubmission.TesId }, { "statusType", StatusType.InvalidUser.ToString() },
                                { "description", "" }
                            }).Result;
                    }
                    else
                    {
                        // The TES message
                        var tesMessage = JsonConvert.DeserializeObject<TesTask>(aSubmission.TesJson);
                        var processedOK = true;

                        // **************  SEND TO RABBIT
                        if (useRabbit)
                        {
                            try
                            {
                                // Not ideal to create each time around the loop but ???
                                IBus rabbit = scope.ServiceProvider.GetRequiredService<IBus>();
                                EasyNetQ.Topology.Exchange exchangeObject = rabbit.Advanced.ExchangeDeclare(ExchangeConstants.Main, "topic");
                                rabbit.Advanced.Publish(exchangeObject, RoutingConstants.Subs, false, new Message<TesTask>(tesMessage));
                            }
                            catch (Exception e)
                            {
                               Log.Error("Send rabbit failed : {message}",e.Message);
                               processedOK = false;
                            }
                        }

                        // **************  SEND TO HUTCH
                        if (useHutch)
                        {
                            // TODO for rest API
                            try
                            {
                                StringContent x = new StringContent("abc");

                                var callHUTCH = treApi.CallAPI("url", x, null,false);
                              
                                // GET task ID

                                // Create HANGFIRE job to check periodically untill we have an completed job

                            }
                            catch (Exception e)
                            {
                                Log.Error("Send HUTCH failed : {message}", e.Message);
                                processedOK = false;
                            }
                        }

                        // **************  SEND TO TESK
                        if (useTESK)
                        {
                            // TODO RESTAPI and hangfire job to follow up
                            try
                            {
                                StringContent x = new StringContent("abc");
                                var callTESK = treApi.CallAPI("url", x,null, false);

                                //  curl - iv - X POST - s--header 'Content-Type: application/json'--header 'Accept: application/json' - d "@test.json" "https://tesk.test-tesk.dk.serp.ac.uk/ga4gh/tes/v1/tasks"

                            }
                            catch (Exception e)
                            {
                                Log.Error("Send TESK failed : {message}", e.Message);
                                processedOK = false;
                            }

                        }

                        // **************  TELL SUBMISSION LAYER WE DONE
                        if (processedOK)
                        {
                            try
                            {
                                var result = treApi.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                                    new Dictionary<string, string>()
                                    {
                                        { "tesId", aSubmission.TesId },
                                        { "statusType", StatusType.TransferredToPod.ToString() },
                                        { "description", "" }
                                    }).Result;
                            }
                            catch (Exception e)
                            {
                                Log.Error("Send record outcome to submission layer : {message}", e.Message);
                                processedOK = false;
                            }
                         
                        }

                    }
                }
            }
            
        }

   
    }
}