using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace Backend.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Kullanıcı bağlandığında ID'sini loglayabilir veya takip edebiliriz.
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    }
}
