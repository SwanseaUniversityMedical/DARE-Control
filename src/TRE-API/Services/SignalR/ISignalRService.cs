using Microsoft.AspNetCore.SignalR;

namespace TRE_API.Services.SignalR
{
    public interface ISignalRService
    {
        Task SendUpdateMessage(string updateName);
    }

    public class SignalRService : Hub, ISignalRService
    {
        private readonly IHubContext<SignalRService> _hubContext;

        public SignalRService(IHubContext<SignalRService> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendUpdateMessage(string updateName)
        {
            await _hubContext.Clients.All.SendAsync(updateName, "Var1", "Var2");
        }
    }
}
