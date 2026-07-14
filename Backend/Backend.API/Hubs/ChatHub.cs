using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using Backend.Application.Abstractions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;

namespace Backend.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatHub(IGenericRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        try
        {
            var user_id = Context.User!.GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(user_id);
            if (user != null)
            {
                user.IsOnline = true;
                user.LastSeenAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.CommitAsync();
                await Clients.Others.SendAsync("UserOnlineStatusChanged", user.Id.ToString(), true, user.LastSeenAt);
            }
        }
        catch { }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var user_id = Context.User!.GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(user_id);
            if (user != null)
            {
                user.IsOnline = false;
                user.LastSeenAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.CommitAsync();
                await Clients.Others.SendAsync("UserOnlineStatusChanged", user.Id.ToString(), false, user.LastSeenAt);
            }
        }
        catch { }
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
