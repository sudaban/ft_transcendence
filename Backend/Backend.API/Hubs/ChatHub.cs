using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Backend.Application.Abstractions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Backend.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<int, HashSet<string>> _userConnections = new();
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<ChatRoomMember> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IGenericRepository<User> userRepository,
        IGenericRepository<ChatRoomMember> memberRepository,
        IUnitOfWork unitOfWork,
        ILogger<ChatHub> logger)
    {
        _userRepository = userRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        try
        {
            var user_id = Context.User!.GetCurrentUserId();
            var connections = _userConnections.GetOrAdd(user_id, _ => new HashSet<string>());
            bool isFirstConnection;
            lock (connections)
            {
                isFirstConnection = connections.Count == 0;
                connections.Add(Context.ConnectionId);
            }

            if (isFirstConnection)
            {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling user connection status in ChatHub");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var user_id = Context.User!.GetCurrentUserId();
            bool isLastConnection = false;
            if (_userConnections.TryGetValue(user_id, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        isLastConnection = true;
                        _userConnections.TryRemove(user_id, out _);
                    }
                }
            }

            if (isLastConnection)
            {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling user disconnection status in ChatHub");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        if (int.TryParse(roomId, out int parsedRoomId))
        {
            var userId = Context.User!.GetCurrentUserId();
            bool isMember = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(
                _memberRepository.TableNoTracking,
                m => m.ChatRoomId == parsedRoomId && m.UserId == userId);

            if (!isMember)
            {
                _logger.LogWarning("User {UserId} unauthorized attempt to join room {RoomId}", userId, roomId);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    }
}
