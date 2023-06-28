using BL.Rabbit;
using EasyNetQ;
using Newtonsoft.Json;
using Serilog;
using System.Text;
using BL.Models;
using System;
using BL.Repositories.DbContexts;
using BL.Models.Tes;
using EasyNetQ.Management.Client.Model;
using Microsoft.EntityFrameworkCore;

namespace DARE_API.Services
{
    public class ConsumeInternalMessageService : BackgroundService
    {
        private readonly IBus _bus;
        private readonly ApplicationDbContext _dbContext;


        public ConsumeInternalMessageService(IBus bus , IServiceProvider serviceProvider)
        {
            _bus = bus;
            _dbContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>(); ;
        
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //Consume All Queue
                var subs = await _bus.Advanced.QueueDeclareAsync(QueueConstants.Submissions);
                _bus.Advanced.Consume<int>(subs, Process);
            }
            catch (Exception e)
            {
                Log.Error("{Function} ConsumeProcessForm:- Failed to subscribe due to error: {e}","ExecuteAsync", e.Message);
                //throw;
            }
        }

        private void Process(IMessage<int> message, MessageReceivedInfo info)
        {
            try
            {
                var sub = _dbContext.Submissions.First(s => s.Id == message.Body);

                //TODO: Validate format of Crate

                var dbproj = sub.Project;
                var tesTask = JsonConvert.DeserializeObject<TesTask>(sub.TesJson);

                var endpointstr = tesTask.Tags.Where(x => x.Key.ToLower() == "endpoints").Select(x => x.Value).FirstOrDefault();
                List<string> endpoints = new List<string>();
                if (!string.IsNullOrWhiteSpace(endpointstr))
                {
                    endpoints = endpointstr.Split('|').Select(x => x.ToLower()).ToList();
                }

                

                var dbendpoints = new List<BL.Models.Endpoint>();

                if (endpoints.Count == 0)
                {
                    dbendpoints = dbproj.Endpoints;
                }
                else
                {
                    foreach (var endpoint in endpoints)
                    {
                        dbendpoints.Add(dbproj.Endpoints.First(x => x.Name.ToLower() == endpoint.ToLower()));
                    }
                }

                sub.Status = SubmissionStatus.WaitingForChildSubsToComplete;
                foreach (var endp in dbendpoints)
                {
                    _dbContext.Add(new Submission()
                    {
                        DockerInputLocation = tesTask.Executors.First().Image,
                        Project = dbproj,
                        Status = SubmissionStatus.WaitingForAgentToTransfer,
                        SubmittedBy = sub.SubmittedBy,
                        Parent = sub,
                        TesId = tesTask.Id,
                        TesJson = sub.TesJson,
                        EndPoint = endp,
                        TesName = tesTask.Name,
                        SourceCrate = tesTask.Executors.First().Image,
                    });

                }

                _dbContext.SaveChanges();
                Log.Information("{Function} Processed sub for {id}", "Process", message.Body);

                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} failed to process.", "Process");
                throw;
            }
        }

       

        private T ConvertByteArrayToType<T>(byte[] byteArray)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
        }
    }
}
