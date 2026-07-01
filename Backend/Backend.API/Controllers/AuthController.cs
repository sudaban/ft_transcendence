using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.DTOs.Responses.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var token = await _authService.RegisterAsync(request);
            return Ok(new { Message = "Registration successful", Token = token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [HttpPost("2fa/setup")]
        [Authorize]
        public async Task<IActionResult> SetupTwoFactor()
        {
            var user_id = GetUserId();
            var result = await _authService.SetupTwoFactorAsync(user_id);
            return Ok(result);
        }

        [HttpPost("2fa/enable")]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequestDto request)
        {
            var user_id = GetUserId();
            var success = await _authService.EnableTwoFactorAsync(user_id, request.Code);
            if (!success)
            {
                return BadRequest(new { Message = "Invalid verification code." });
            }
            return Ok(new { Message = "Two-factor authentication enabled successfully." });
        }

        [HttpPost("2fa/disable")]
        [Authorize]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var user_id = GetUserId();
            var success = await _authService.DisableTwoFactorAsync(user_id);
            if (!success)
            {
                return BadRequest(new { Message = "Failed to disable two-factor authentication." });
            }
            return Ok(new { Message = "Two-factor authentication disabled successfully." });
        }

        [HttpPost("2fa/login")]
        public async Task<IActionResult> TwoFactorLogin([FromBody] TwoFactorLoginRequestDto request)
        {
            var response = await _authService.Verify2FaLoginAsync(request);
            return Ok(response);
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult TestToken()
        {
            var username = User.Identity?.Name;
            return Ok(new { Message = $"Token is completely valid! Welcome to the secure area, {username}." });
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var user_id))
            {
                throw new System.Security.Authentication.AuthenticationException("User is not authenticated.");
            }
            return user_id;
        }
    }
}