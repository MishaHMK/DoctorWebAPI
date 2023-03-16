using Microsoft.AspNetCore.SignalR;

namespace DoctorWebApi.HubSignalR
{
    public class NotficitaionHub : Hub
    {
        public async Task NewMessage(string user, string message)
        {
            await Clients.All.SendAsync("messageReceived", user, message);
        }
    }
}
