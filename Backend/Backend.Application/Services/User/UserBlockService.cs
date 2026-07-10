using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Responses.Users;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Application.Services.Users;

public class UserBlockService : IUserBlockService
{
    private readonly IGenericRepository<UserBlock> _userBlockRepository;
    private readonly IGenericRepository<Follow> _followRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public UserBlockService(
        IGenericRepository<UserBlock> userBlockRepository,
        IGenericRepository<Follow> followRepository,
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _userBlockRepository = userBlockRepository;
        _followRepository = followRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task BlockUserAsync(int targetUserId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        if (currentUserId == targetUserId)
            throw new BadRequestException("You cannot block yourself.");

        var targetUserExists = await _userRepository.TableNoTracking.AnyAsync(u => u.Id == targetUserId);
        if (!targetUserExists)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        var isBlocked = await _userBlockRepository.TableNoTracking
            .AnyAsync(ub => ub.BlockerId == currentUserId && ub.BlockedId == targetUserId);

        if (isBlocked)
            throw new OverlapException("You have already blocked this user.");

        var userBlock = new UserBlock
        {
            BlockerId = currentUserId,
            BlockedId = targetUserId
        };
        await _userBlockRepository.AddAsync(userBlock);

        var followsToRemove = await _followRepository.Table
            .Where(f => (f.FollowerId == currentUserId && f.FollowingId == targetUserId) ||
                        (f.FollowerId == targetUserId && f.FollowingId == currentUserId))
            .ToListAsync();

        foreach (var follow in followsToRemove)
        {
            await _followRepository.DeleteAsync(follow);
        }

        await _unitOfWork.CommitAsync();
    }

    public async Task UnblockUserAsync(int targetUserId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var blockRecord = await _userBlockRepository.Table
            .FirstOrDefaultAsync(ub => ub.BlockerId == currentUserId && ub.BlockedId == targetUserId);

        if (blockRecord == null)
            throw new NotFoundException("You have not blocked this user.");

        await _userBlockRepository.DeleteAsync(blockRecord);
        await _unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<UserDto>> GetBlockedUsersAsync()
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var blockedUsers = await _userBlockRepository.TableNoTracking
            .Where(ub => ub.BlockerId == currentUserId)
            .Select(ub => ub.Blocked)
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserDto>>(blockedUsers);
    }
}
