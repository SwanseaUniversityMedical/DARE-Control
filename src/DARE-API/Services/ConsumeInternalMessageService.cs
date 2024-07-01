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
using BL.Models.ViewModels;
using BL.Services;
using EasyNetQ.Management.Client.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;

namespace DARE_API.Services
{
    public class ConsumeInternalMessageService : BackgroundService
    {
        private readonly IBus _bus;
        private readonly ApplicationDbContext _dbContext;
        private readonly MinioSettings _minioSettings;
        private readonly IMinioHelper _minioHelper;

        public ConsumeInternalMessageService(IBus bus, IServiceProvider serviceProvider)
        {
            _bus = bus;
            _dbContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _minioSettings = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<MinioSettings>();
            _minioHelper = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IMinioHelper>();; 

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //Consume All Queue
                var subs = await _bus.Advanced.QueueDeclareAsync(QueueConstants.ProcessSub);
                _bus.Advanced.Consume<int>(subs, Process);

                //var fetch = await _bus.Advanced.QueueDeclareAsync(QueueConstants.FetchExternalFile);
                //_bus.Advanced.Consume<byte[]>(fetch, ProcessFetchExternal);
            }
            catch (Exception e)
            {
                Log.Error("{Function} ConsumeProcessForm:- Failed to subscribe due to error: {e}", "ExecuteAsync", e.Message);

            }
        }


        //Implement proper check
        private bool ValidateCreate(Submission sub)
        {
            return true;
        }

        private void Process(IMessage<int> message, MessageReceivedInfo info)
        {
            try
            {
                var sub = _dbContext.Submissions.First(s => s.Id == message.Body);

                try
                {
                    
              

               
                

                var messageMQ = new MQFetchFile();
                messageMQ.Url = sub.SourceCrate;
                messageMQ.BucketName = sub.Project.SubmissionBucket;

                    Uri uri = null;

                    try
                    {
                        uri = new Uri(sub.DockerInputLocation);
                    }
                    catch (Exception ex)
                    {

                    }
                    Log.Information("{Function} Crate loc {Crate}", "Process", sub.DockerInputLocation);
                    if (uri != null)
                    {
                        
                        string fileName = Path.GetFileName(uri.LocalPath);
                        Log.Information("{Function} Full file loc {File}, Incoming URL {URL}, our minio {Minio}", "Process", fileName, uri.Host + ":" + uri.Port, _minioSettings.AdminConsole);
                        messageMQ.Key = fileName;
                        if (uri.Host + ":" + uri.Port != _minioSettings.AdminConsole)
                        {
                            Log.Information("{Function} Copying external", "Process");
                            _minioHelper.RabbitExternalObject(messageMQ);


                            var minioEndpoint = new MinioEndpoint()
                            {
                                Url = _minioSettings.AdminConsole,
                            };
                            messageMQ.Url = "http://" + minioEndpoint.Url + "/browser/" + messageMQ.BucketName + "/" + messageMQ.Key;
                            Log.Information("{Function} New url {URL}", "Process", messageMQ.Url);
                        }
                    }
                   

                    UpdateSubmissionStatus.UpdateStatusNoSave(sub, StatusType.SubmissionWaitingForCrateFormatCheck, "");
                if (ValidateCreate(sub))
                {
                    UpdateSubmissionStatus.UpdateStatusNoSave(sub, StatusType.SubmissionCrateValidated, "");
                }
                else
                {
                    UpdateSubmissionStatus.UpdateStatusNoSave(sub, StatusType.SubmissionCrateValidationFailed, "");
                    UpdateSubmissionStatus.UpdateStatusNoSave(sub, StatusType.Failed, "");
                }

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
                UpdateSubmissionStatus.UpdateStatusNoSave(sub, StatusType.WaitingForChildSubsToComplete, "");

                foreach (var tre in dbtres)
                {
                    _dbContext.Add(new Submission()
                    {
                        DockerInputLocation = messageMQ.Url,
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
                catch (Exception e)
                {
                    UpdateSubmissionStatus.UpdateStatusNoSave(sub, StatusType.Failed, e.Message);
                    _dbContext.SaveChanges();

                    
                    
                    throw;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} failed to process.", "Process");
                throw;
            }
        }

        //private async Task ProcessFetchExternal(IMessage<byte[]> msgBytes,   MessageReceivedInfo info )
        //{
        //    try
        //    {
        //        var message = Encoding.UTF8.GetString(msgBytes.Body);
        //        await _minioHelper.RabbitExternalObject(JsonConvert.DeserializeObject<MQFetchFile>(message));
        //    }
        //    catch (Exception e)
        //    {

        //        throw;
        //    }
        //}




        private T ConvertByteArrayToType<T>(byte[] byteArray)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
        }
    }
}
