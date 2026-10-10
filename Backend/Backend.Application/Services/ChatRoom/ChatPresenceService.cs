using System;
using System.Threading.Tasks;
using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Services.ChatRooms;

public class ChatPresenceService : IChatPresenceService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<ChatRoomMember> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatPresenceService(
        IGenericRepository<User> userRepository,
        IGenericRepository<ChatRoomMember> memberRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DateTime?> SetUserOnlineStatusAsync(int userId, bool isOnline)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        user.IsOnline = isOnline;
        user.LastSeenAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();
        return user.LastSeenAt;
    }

    public async Task<bool> IsRoomMemberAsync(int roomId, int userId)
    {
        return await _memberRepository.TableNoTracking
            .AnyAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
    }
}
