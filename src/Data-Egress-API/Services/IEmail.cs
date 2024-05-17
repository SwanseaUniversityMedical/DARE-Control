using BL.Models;
using NETCore.MailKit.Core;
using Org.BouncyCastle.Asn1.Crmf;

namespace DARE_Egress.Services
{
    public interface IDareEmailService
    {
        Task EmailTo(string emailTo, string Subject, string body, bool IsHtml);
    }

    public class DareEmailService : IDareEmailService
    {
        private IEmailService _IEmailService;
        private EmailSettings _EmailSettings;

        public DareEmailService(IEmailService IEmailService, EmailSettings emailSettings)
        {

            _IEmailService = IEmailService;
            _EmailSettings = emailSettings;
        }

        public async Task EmailTo(string emailTo, string Subject, string body, bool IsHtml)
        {
            if (_EmailSettings.Enabled)
            {
                await _IEmailService.SendAsync(emailTo, Subject, body, IsHtml);
            }
        }
    }
}
