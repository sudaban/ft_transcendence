using Backend.Application.DTOs.Responses.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IUserBlockService
{
    Task BlockUserAsync(int targetUserId);
    Task UnblockUserAsync(int targetUserId);
    Task<IEnumerable<UserDto>> GetBlockedUsersAsync();
    Task<bool> HasBlockRelationshipAsync(int userA, int userB);
    Task<List<int>> GetBlockedUserIdsAsync(int userId);
}
