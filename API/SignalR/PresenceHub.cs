using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User == null)
            throw new HubException("Cannot get current user claim");

        await presenceTracker.UserConnected(Context.User.GetUserName(), Context.ConnectionId);
        await Clients.Others.SendAsync("UserIsOnline", Context?.User?.GetUserName());

        var onlineUsers = presenceTracker.GetOnlineUsers();

        await Clients.All.SendAsync("GetOnlineUsers", onlineUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User == null)
            throw new HubException("Cannot get current user claim");

        await presenceTracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);
        await Clients.Others.SendAsync("UserIsOffline", Context?.User?.GetUserName());

        var onlineUsers = presenceTracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", onlineUsers);

        await base.OnDisconnectedAsync(exception);
    }
}
