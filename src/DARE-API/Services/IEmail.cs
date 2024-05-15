using NETCore.MailKit.Core;
using Org.BouncyCastle.Asn1.Crmf;
using RabbitMQ.Client;

namespace DARE_API.Services
{
    public interface IDareEmailService
    {
        Task EmailTo(string emailTo, string Subject, string body, bool IsHtml);
    }

    public class DareEmailService : IDareEmailService
    {
        private IEmailService _IEmailService;


        public DareEmailService(IEmailService IEmailService) { 
        
            _IEmailService = IEmailService;

        }

        public async Task EmailTo(string emailTo, string Subject, string body, bool IsHtml)
        {
            await _IEmailService.SendAsync(emailTo, Subject, body, IsHtml);
        }
    }
}
