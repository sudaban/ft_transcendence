using Backend.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserBlocksController : ControllerBase
{
    private readonly IUserBlockService _userBlockService;

    public UserBlocksController(IUserBlockService userBlockService)
    {
        _userBlockService = userBlockService;
    }

    [HttpPost("{targetUserId}")]
    [Authorize]
    public async Task<IActionResult> BlockUser(int targetUserId)
    {
        await _userBlockService.BlockUserAsync(targetUserId);
        return Ok(new { Message = "User has been blocked successfully." });
    }

    [HttpDelete("{targetUserId}")]
    [Authorize]
    public async Task<IActionResult> UnblockUser(int targetUserId)
    {
        await _userBlockService.UnblockUserAsync(targetUserId);
        return Ok(new { Message = "User has been unblocked successfully." });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetBlockedUsers()
    {
        var blockedUsers = await _userBlockService.GetBlockedUsersAsync();
        return Ok(blockedUsers);
    }
}
