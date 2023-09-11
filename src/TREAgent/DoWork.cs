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
using System.Net.Http.Headers;
using System.Text;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;
using TREAgent.Repositories;
using TREAgent.Repositories.DbContexts;

namespace TREAgent
{
    public interface IDoWork
    {
        void Execute();
        void CheckTESK(string taskID);
        void testing();
    }

    // TESK : http://172.16.34.31:8080/    https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks


    public class DoWork : IDoWork
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbContext;
        public DoWork(IServiceProvider serviceProvider, ApplicationDbContext dbContext)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
        }

        public void testing()
        {

            Console.WriteLine("Testing");
            string jsonContent = "{\"name\": \"Hello World\",\r\n  \"description\": \"Hello World, inspired by Funnel's most basic example\",\r\n  \"executors\": [\r\n    {\r\n      \"image\": \"alpine\",\r\n      \"command\": [\r\n        \"echo\",\r\n        \"TESK says: Hello World\"\r\n      ]\r\n    }\r\n  ]}"; // Replace with your JSON data

            CreateTESK(jsonContent);
        }

        public string CreateTESK(string jsonContent)
        {
            using (var httpClient = new HttpClient())
            {
                // Define the URL for the POST request
                string apiUrl = "https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks";

                // Create a HttpRequestMessage with the HTTP method set to POST
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Set the headers
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                // Attach the JSON string to the request's content
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


                // Send the POST request
                HttpResponseMessage response = httpClient.SendAsync(request).Result;

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Request successful. Response:");
                    Console.WriteLine(responseBody);
                    Log.Information("Request successful. Response: {response}", responseBody);

                    var responseObj = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                    string id = responseObj.id;

                    RecurringJob.AddOrUpdate<IDoWork>(id, a => a.CheckTESK(id), Cron.MinuteInterval(1));
                    
                    return id;
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    return "";
                }
            }
        }

        class ResponseModel
        {
            public string id { get; set; }
        }
        public void CheckTESK(string taskID)
        {
            Console.WriteLine("Check Task : "+taskID);

            string url = "https://tesk.ukserp.ac.uk/ga4gh/tes/v1/tasks/"+taskID+"?view=basic";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response =  client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string content =   response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(content);
                    TESKstatus status = JsonConvert.DeserializeObject<TESKstatus>(content);

                  
                        var shouldReport = false;

                   
                        var fromDatabase = (_dbContext.TESK_Status.Where(x => x.id == taskID)).FirstOrDefault();

                        if (fromDatabase is null)
                        {
                            shouldReport = true;
                           _dbContext.Add(status);
                        }
                        else
                        {
                            if (fromDatabase.state != status.state)
                            {
                                shouldReport = true;
                                fromDatabase.state=status.state;
                               _dbContext.Update(fromDatabase);
                            }
                        }

                        if (shouldReport==true)
                        {
                            Console.WriteLine("*** status change *** ");
                            _dbContext.SaveChanges();

                        } else Console.WriteLine("NO CHANGE");
                  

                }
                else
                {
                    Console.WriteLine($"HTTP Request {url} failed with status code: {response.StatusCode}");
                }
            }


            RecurringJob.RemoveIfExists(taskID);
        }



        // Method executed upon hangfire job
        public void Execute()
        {
            // control use of dependency injection
            using (var scope = _serviceProvider.CreateScope())
            {

                
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
                            if (tesMessage is not null)
                                CreateTESK(tesMessage);
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