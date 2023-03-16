using Microsoft.AspNetCore.SignalR;
using Doctor.BLL.Extensions;

namespace DoctorWebApi.HubSignalR
{
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker; 
        }

        public override async Task OnConnectedAsync()
        {
            await _tracker.UserConnected(Context.User.GetNameId(), Context.ConnectionId);
            await Clients.All.SendAsync("UserIsOnline", Context.User.GetNameId());

            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _tracker.UserDisconnected(Context.User.GetNameId(), Context.ConnectionId);
            await Clients.All.SendAsync("UserIsOffline", Context.User.GetNameId());

            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            await base.OnDisconnectedAsync(exception);  
        }
    }
}
