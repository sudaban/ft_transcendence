using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Users;
using Backend.Application.DTOs.Responses.Users;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IGenericRepository<Backend.Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IGenericRepository<Backend.Domain.Entities.User> userRepository, IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var is_admin = _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;
        IQueryable<Backend.Domain.Entities.User> query = _userRepository.TableNoTracking
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts);

        if (is_admin)
        {
            query = query.IgnoreQueryFilters();
        }

        var users = await query.ToListAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var is_admin = _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;
        IQueryable<Backend.Domain.Entities.User> query = _userRepository.TableNoTracking
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .Where(u => u.Id == id);

        if (is_admin)
        {
            query = query.IgnoreQueryFilters();
        }

        var user = await query.FirstOrDefaultAsync();
        if (user == null)
            throw new NotFoundException($"User with ID {id} not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetUserByUsernameAsync(string username)
    {
        var is_admin = _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;
        IQueryable<Backend.Domain.Entities.User> query = _userRepository.TableNoTracking
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .Where(u => u.Username == username);

        if (is_admin)
        {
            query = query.IgnoreQueryFilters();
        }

        var user = await query.FirstOrDefaultAsync();
        
        if (user == null)
            throw new NotFoundException($"User with username '{username}' not found.");

        return _mapper.Map<UserDto>(user);
    }


    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequestDto request)
    {
        int user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var user = await _userRepository.Table
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .FirstOrDefaultAsync(u => u.Id == user_id);
        if (user == null)
            throw new NotFoundException($"User with ID {user_id} not found.");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Bio != null) user.Bio = request.Bio;
        if (request.ProfilePictureUrl != null) user.ProfilePictureUrl = request.ProfilePictureUrl;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteUserAsync()
    {
        int user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var user = await _userRepository.GetByIdAsync(user_id);
        if (user == null)
            throw new NotFoundException($"User with ID {user_id} not found.");

        user.IsDeleted = true;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();
    }

    public async Task AdminDeleteUserAsync(int targetUserId)
    {
        _httpContextAccessor.HttpContext!.User.CheckIfAdmin();

        var target_user = await _userRepository.GetByIdAsync(targetUserId);
        if (target_user == null)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        target_user.IsDeleted = true;
        await _userRepository.UpdateAsync(target_user);
        await _unitOfWork.CommitAsync();
    }

    public async Task AdminBanUserAsync(int targetUserId, bool isBanned)
    {
        _httpContextAccessor.HttpContext!.User.CheckIfAdmin();

        var target_user = await _userRepository.GetByIdAsync(targetUserId);
        if (target_user == null)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        target_user.IsBanned = isBanned;
        await _userRepository.UpdateAsync(target_user);
        await _unitOfWork.CommitAsync();
    }
}
