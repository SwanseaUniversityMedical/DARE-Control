using BL.Rabbit;
using EasyNetQ;
using Newtonsoft.Json;
using Serilog;
using System.Text;
using BL.Models;
using System;
using BL.Models.Enums;
using BL.Models.Tes;
using TRE_API.Repositories.DbContexts;
using BL.Services;
using BL.Models.ViewModels;

namespace TRE_API.Services
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
            _minioHelper = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IMinioHelper>();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //Consume All Queue
                var fetch = await _bus.Advanced.QueueDeclareAsync(QueueConstants.FetchExtarnalFile);
                _bus.Advanced.Consume<MQFetchFile>(fetch, Process);
            }
            catch (Exception e)
            {
                Log.Error("{Function} ConsumeProcessForm:- Failed to subscribe due to error: {e}","ExecuteAsync", e.Message);
                
            }
        }

        private async Task Process(IMessage<MQFetchFile> message, MessageReceivedInfo info)
        {
            try
            {
                var messageMQ = message.Body;
                await _minioHelper.RabbitExternalObject(messageMQ);
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
