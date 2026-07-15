using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpGet("username/{username}")]
    [Authorize]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        return Ok(user);
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        var users = await _userService.SearchUsersAsync(query);
        return Ok(users);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        var user = await _userService.UpdateProfileAsync(request);
        return Ok(user);
    }

    [HttpPost("profile/avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(Microsoft.AspNetCore.Http.IFormFile file)
    {
        var user = await _userService.UpdateAvatarAsync(file);
        return Ok(user);
    }

    [HttpDelete("profile")]
    [Authorize]
    public async Task<IActionResult> DeleteProfile()
    {
        await _userService.DeleteUserAsync();
        return Ok(new { Message = "Your profile has been soft-deleted successfully." });
    }

    [HttpDelete("admin/{targetUserId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDeleteUser(int targetUserId)
    {
        await _userService.AdminDeleteUserAsync(targetUserId);
        return Ok(new { Message = "User has been deleted successfully." });
    }

    [HttpPost("admin/ban/{targetUserId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminBanUser(int targetUserId, [FromQuery] bool isBanned)
    {
        await _userService.AdminBanUserAsync(targetUserId, isBanned);
        return Ok(new { Message = $"User ban status updated to {isBanned}." });
    }
}
