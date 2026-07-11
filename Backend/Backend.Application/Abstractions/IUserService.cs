using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Application.DTOs.Requests.Users;
using Backend.Application.DTOs.Responses.Users;

namespace Backend.Application.Abstractions;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(int id);
    Task<UserDto> GetUserByUsernameAsync(string username);
    Task<UserDto> UpdateProfileAsync(UpdateProfileRequestDto request);
    Task<UserDto> UpdateAvatarAsync(Microsoft.AspNetCore.Http.IFormFile file);
    Task DeleteUserAsync();
    
    // Admin yetkili metotlar
    Task AdminDeleteUserAsync(int targetUserId);
    Task AdminBanUserAsync(int targetUserId, bool isBanned);
}
