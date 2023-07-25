using Microsoft.AspNetCore.SignalR;

namespace TRE_API.Services.SignalR
{
    public interface ISignalRService
    {
        Task SendUpdateMessage(string updateName, string endpointName, string treId, string submissionStatus);
    }

    public class SignalRService : Hub, ISignalRService
    {
        private readonly IHubContext<SignalRService> _hubContext;

        public SignalRService(IHubContext<SignalRService> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendUpdateMessage(string updateName, string endpointName, string treId, string submissionStatus)
        {
            await _hubContext.Clients.All.SendAsync(updateName, endpointName, treId, submissionStatus);
        }
    }
}
