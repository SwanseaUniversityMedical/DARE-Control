using BL.Rabbit;
using EasyNetQ;
using Newtonsoft.Json;
using Serilog;
using System.Text;
using BL.Models;
using System;
using BL.Models.Enums;
using DARE_API.Repositories.DbContexts;
using BL.Models.Tes;
using BL.Services;

namespace DARE_API.Services
{
    public class ConsumeInternalMessageService : BackgroundService
    {
        private readonly IBus _bus;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMinioHelper _minioHelper;


        public ConsumeInternalMessageService(IBus bus , IServiceProvider serviceProvider)
        {
            _bus = bus;
            _dbContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>(); 
            _minioHelper=serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IMinioHelper>();
        
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //Consume All Queue
                var subs = await _bus.Advanced.QueueDeclareAsync(QueueConstants.Submissions);
                _bus.Advanced.Consume<int>(subs, Process);

                var fetch = await _bus.Advanced.QueueDeclareAsync(QueueConstants.FetchExtarnalFile);
                _bus.Advanced.Consume<byte[]>(fetch, ProcessFetchExternal);
            }
            catch (Exception e)
            {
                Log.Error("{Function} ConsumeProcessForm:- Failed to subscribe due to error: {e}","ExecuteAsync", e.Message);
                
            }
        }

        private void Process(IMessage<int> message, MessageReceivedInfo info)
        {
            try
            {
                var sub = _dbContext.Submissions.First(s => s.Id == message.Body);

                //TODO: Mahadi copy crate from external to local submission (if in external)

                //TODO: Validate format of Crate

                var dbproj = sub.Project;
                var tesTask = JsonConvert.DeserializeObject<TesTask>(sub.TesJson);

                var trestr = tesTask.Tags.Where(x => x.Key.ToLower() == "tres").Select(x => x.Value).FirstOrDefault();
                List<string> tres = new List<string>();
                if (!string.IsNullOrWhiteSpace(trestr))
                {
                    tres = trestr.Split('|').Select(x => x.ToLower()).ToList();
                }

                

                var dbtres = new List<BL.Models.Tre>();

                if (tres.Count == 0)
                {
                    dbtres = dbproj.Tres;
                }
                else
                {
                    foreach (var tre in tres)
                    {
                        dbtres.Add(dbproj.Tres.First(x => x.Name.ToLower() == tre.ToLower()));
                    }
                }
                UpdateSubmissionStatus.UpdateStatus(sub, StatusType.WaitingForChildSubsToComplete, "");
                
                foreach (var tre in dbtres)
                {
                    _dbContext.Add(new Submission()
                    {
                        DockerInputLocation = tesTask.Executors.First().Image,
                        Project = dbproj,
                        StartTime = DateTime.Now.ToUniversalTime(),
                        Status = StatusType.WaitingForAgentToTransfer,
                        LastStatusUpdate = DateTime.Now.ToUniversalTime(),
                        SubmittedBy = sub.SubmittedBy,
                        Parent = sub,
                        TesId = tesTask.Id,
                        TesJson = sub.TesJson,
                        Tre = tre,
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

        private async Task ProcessFetchExternal(IMessage<byte[]> msgBytes,   MessageReceivedInfo info )
        {
            try
            {
                var message = Encoding.UTF8.GetString(msgBytes.Body);
                await  _minioHelper.RabbitExternalObject(message);
            }
            catch (Exception e)
            {

                throw;
            }
        }




        private T ConvertByteArrayToType<T>(byte[] byteArray)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
        }
    }
}
