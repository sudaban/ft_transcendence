using System;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IChatPresenceService
{
    Task<DateTime?> SetUserOnlineStatusAsync(int userId, bool isOnline);
    Task<bool> IsRoomMemberAsync(int roomId, int userId);
}
