using Backend.Application.DTOs.Responses.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IFollowService
{
    Task FollowUserAsync(int targetUserId);
    Task UnfollowUserAsync(int targetUserId);
    Task<IEnumerable<UserDto>> GetFollowersAsync(int userId);
    Task<IEnumerable<UserDto>> GetFollowingAsync(int userId);
}
