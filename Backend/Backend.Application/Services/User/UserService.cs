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
        var isAdmin = _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;
        var query = _userRepository.TableNoTracking;

        if (isAdmin)
        {
            query = query.IgnoreQueryFilters();
        }

        var users = await query.ToListAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var isAdmin = _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;
        var query = _userRepository.TableNoTracking.Where(u => u.Id == id);

        if (isAdmin)
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
        var isAdmin = _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;
        var query = _userRepository.TableNoTracking.Where(u => u.Username == username);

        if (isAdmin)
        {
            query = query.IgnoreQueryFilters();
        }

        var user = await query.FirstOrDefaultAsync();
        
        if (user == null)
            throw new NotFoundException($"User with username '{username}' not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<DatabaseUserDto> GetDatabaseUserByIdAsync(int id)
    {
        var isAdmin = _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;
        var query = _userRepository.TableNoTracking.Where(u => u.Id == id);

        if (isAdmin)
        {
            query = query.IgnoreQueryFilters();
        }

        var user = await query.FirstOrDefaultAsync();
        if (user == null)
            throw new NotFoundException($"User with ID {id} not found.");

        return _mapper.Map<DatabaseUserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequestDto request)
    {
        int userId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with ID {userId} not found.");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Bio != null) user.Bio = request.Bio;
        if (request.ProfilePictureUrl != null) user.ProfilePictureUrl = request.ProfilePictureUrl;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteUserAsync()
    {
        int userId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with ID {userId} not found.");

        // Soft delete
        user.IsDeleted = true;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();
    }

    public async Task AdminDeleteUserAsync(int targetUserId)
    {
        _httpContextAccessor.HttpContext!.User.CheckIfAdmin();

        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        targetUser.IsDeleted = true;
        await _userRepository.UpdateAsync(targetUser);
        await _unitOfWork.CommitAsync();
    }

    public async Task AdminBanUserAsync(int targetUserId, bool isBanned)
    {
        _httpContextAccessor.HttpContext!.User.CheckIfAdmin();

        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        targetUser.IsBanned = isBanned;
        await _userRepository.UpdateAsync(targetUser);
        await _unitOfWork.CommitAsync();
    }
}
