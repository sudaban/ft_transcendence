using Backend.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowsController(IFollowService followService)
    {
        _followService = followService;
    }

    [HttpPost("{targetUserId}")]
    [Authorize]
    public async Task<IActionResult> FollowUser(int targetUserId)
    {
        await _followService.FollowUserAsync(targetUserId);
        return Ok(new { Message = "User followed successfully." });
    }

    [HttpDelete("{targetUserId}")]
    [Authorize]
    public async Task<IActionResult> UnfollowUser(int targetUserId)
    {
        await _followService.UnfollowUserAsync(targetUserId);
        return Ok(new { Message = "User unfollowed successfully." });
    }

    [HttpGet("{userId}/followers")]
    [Authorize]
    public async Task<IActionResult> GetFollowers(int userId)
    {
        var followers = await _followService.GetFollowersAsync(userId);
        return Ok(followers);
    }

    [HttpGet("{userId}/following")]
    [Authorize]
    public async Task<IActionResult> GetFollowing(int userId)
    {
        var following = await _followService.GetFollowingAsync(userId);
        return Ok(following);
    }
}
