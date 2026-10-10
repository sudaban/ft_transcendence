using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Users;
using Backend.Application.DTOs.Responses.Users;
using Backend.Application.Exceptions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IGenericRepository<Backend.Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileUploadService _file_upload_service;

    public UserService(
        IGenericRepository<Backend.Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IFileUploadService file_upload_service)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _file_upload_service = file_upload_service;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var is_admin = _currentUserService.IsAdmin;
        IQueryable<Backend.Domain.Entities.User> query = _userRepository.TableNoTracking
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .AsSplitQuery();

        if (is_admin)
        {
            query = query.IgnoreQueryFilters();
        }

        var users = await query.ToListAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var is_admin = _currentUserService.IsAdmin;
        IQueryable<Backend.Domain.Entities.User> query = _userRepository.TableNoTracking
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .AsSplitQuery()
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
        var is_admin = _currentUserService.IsAdmin;
        IQueryable<Backend.Domain.Entities.User> query = _userRepository.TableNoTracking
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .AsSplitQuery()
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

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<UserDto>();
        }

        query = query.Trim();

        var users = await _userRepository.TableNoTracking
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .Where(u => EF.Functions.Like(u.Username.ToLower(), $"{query.ToLower()}%"))
            .Take(10)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequestDto request)
    {
        int user_id = _currentUserService.UserId;
        var user = await _userRepository.Table
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .FirstOrDefaultAsync(u => u.Id == user_id);
        if (user == null)
            throw new NotFoundException($"User with ID {user_id} not found.");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Bio != null) user.Bio = request.Bio;
        if (request.ProfilePictureUrl != null)
        {
            if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
            {
                bool isValidUrl = Uri.TryCreate(request.ProfilePictureUrl, UriKind.Absolute, out var uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                bool isValidRelative = request.ProfilePictureUrl.StartsWith("/uploads/") || request.ProfilePictureUrl.StartsWith("/");
                if (!isValidUrl && !isValidRelative)
                {
                    throw new BadRequestException("Invalid profile picture URL.");
                }
            }
            user.ProfilePictureUrl = request.ProfilePictureUrl;
        }

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateAvatarAsync(IFormFile file)
    {
        int user_id = _currentUserService.UserId;
        var user = await _userRepository.Table
            .Include(u => u.FollowedBy)
            .Include(u => u.Following)
            .Include(u => u.Posts)
            .FirstOrDefaultAsync(u => u.Id == user_id);
        if (user == null)
            throw new NotFoundException($"User with ID {user_id} not found.");

        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            _file_upload_service.DeleteFile(user.ProfilePictureUrl);
        }

        using var stream = file.OpenReadStream();
        var file_url = await _file_upload_service.UploadFileAsync(stream, file.FileName, file.ContentType);
        user.ProfilePictureUrl = file_url;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteUserAsync()
    {
        int user_id = _currentUserService.UserId;
        var user = await _userRepository.GetByIdAsync(user_id);
        if (user == null)
            throw new NotFoundException($"User with ID {user_id} not found.");

        user.IsDeleted = true;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();
    }

    public async Task AdminDeleteUserAsync(int targetUserId)
    {
        _currentUserService.CheckIfAdmin();

        var target_user = await _userRepository.GetByIdAsync(targetUserId);
        if (target_user == null)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        target_user.IsDeleted = true;
        await _userRepository.UpdateAsync(target_user);
        await _unitOfWork.CommitAsync();
    }

    public async Task AdminBanUserAsync(int targetUserId, bool isBanned)
    {
        _currentUserService.CheckIfAdmin();

        var target_user = await _userRepository.GetByIdAsync(targetUserId);
        if (target_user == null)
            throw new NotFoundException($"User with ID {targetUserId} not found.");

        target_user.IsBanned = isBanned;
        await _userRepository.UpdateAsync(target_user);
        await _unitOfWork.CommitAsync();
    }
}
