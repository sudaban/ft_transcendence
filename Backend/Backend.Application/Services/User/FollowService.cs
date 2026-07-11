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

public class FollowService : IFollowService
{
    private readonly IGenericRepository<Follow> _followRepository;
    private readonly IGenericRepository<UserBlock> _userBlockRepository;
    private readonly IGenericRepository<Backend.Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public FollowService(
        IGenericRepository<Follow> followRepository,
        IGenericRepository<UserBlock> userBlockRepository,
        IGenericRepository<Backend.Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task FollowUserAsync(int targetUserId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        if (currentUserId == targetUserId)
            throw new BadRequestException("You cannot follow yourself.");

        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        bool hasBlock = await _userBlockRepository.TableNoTracking
            .AnyAsync(ub => (ub.BlockerId == currentUserId && ub.BlockedId == targetUserId) ||
                            (ub.BlockerId == targetUserId && ub.BlockedId == currentUserId));

        if (hasBlock)
            throw new UnAuthorizedAccessException("You cannot follow this user due to a block.");

        bool isAlreadyFollowing = await _followRepository.TableNoTracking
            .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

        if (isAlreadyFollowing)
            throw new OverlapException("You are already following this user.");

        var follow = new Follow
        {
            FollowerId = currentUserId,
            FollowingId = targetUserId
        };

        await _followRepository.AddAsync(follow);
        await _unitOfWork.CommitAsync();
    }

    public async Task UnfollowUserAsync(int targetUserId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var followRecord = await _followRepository.Table
            .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

        if (followRecord == null)
            throw new NotFoundException("You are not following this user.");

        await _followRepository.DeleteAsync(followRecord);
        await _unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<UserDto>> GetFollowersAsync(int userId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var query = _followRepository.TableNoTracking
            .Where(f => f.FollowingId == userId)
            .Include(f => f.Follower)
            .Select(f => f.Follower)
            .AsQueryable();

        if (!isAdmin)
        {
            var blockedIds = await _userBlockRepository.TableNoTracking
                .Where(ub => ub.BlockerId == currentUserId || ub.BlockedId == currentUserId)
                .Select(ub => ub.BlockerId == currentUserId ? ub.BlockedId : ub.BlockerId)
                .ToListAsync();

            query = query.Where(u => !blockedIds.Contains(u.Id));
        }

        var followers = await query.ToListAsync();
        return _mapper.Map<IEnumerable<UserDto>>(followers);
    }

    public async Task<IEnumerable<UserDto>> GetFollowingAsync(int userId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var query = _followRepository.TableNoTracking
            .Where(f => f.FollowerId == userId)
            .Include(f => f.Following)
            .Select(f => f.Following)
            .AsQueryable();

        if (!isAdmin)
        {
            var blockedIds = await _userBlockRepository.TableNoTracking
                .Where(ub => ub.BlockerId == currentUserId || ub.BlockedId == currentUserId)
                .Select(ub => ub.BlockerId == currentUserId ? ub.BlockedId : ub.BlockerId)
                .ToListAsync();

            query = query.Where(u => !blockedIds.Contains(u.Id));
        }

        var following = await query.ToListAsync();
        return _mapper.Map<IEnumerable<UserDto>>(following);
    }
}
