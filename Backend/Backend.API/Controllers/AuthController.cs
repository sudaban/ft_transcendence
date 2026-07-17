using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.DTOs.Responses.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;

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

        [HttpPost("oauth/login")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequestDto request)
        {
            var response = await _authService.OAuthLoginAsync(request);
            return Ok(response);
        }

        [HttpPost("2fa/setup")]
        [Authorize]
        public async Task<IActionResult> SetupTwoFactor()
        {
            var result = await _authService.SetupTwoFactorAsync(User.GetCurrentUserId());
            return Ok(result);
        }

        [HttpPost("2fa/enable")]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequestDto request)
        {
            var success = await _authService.EnableTwoFactorAsync(User.GetCurrentUserId(), request.Code);
            if (!success)
            {
                throw new BadRequestException("Invalid verification code.");
            }
            return Ok(new { Message = "Two-factor authentication enabled successfully." });
        }

        [HttpPost("2fa/disable")]
        [Authorize]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var success = await _authService.DisableTwoFactorAsync(User.GetCurrentUserId());
            if (!success)
            {
                throw new BadRequestException("Failed to disable two-factor authentication.");
            }
            return Ok(new { Message = "Two-factor authentication disabled successfully." });
        }

        [HttpPost("2fa/login")]
        public async Task<IActionResult> TwoFactorLogin([FromBody] TwoFactorLoginRequestDto request)
        {
            var response = await _authService.Verify2FaLoginAsync(request);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequestDto request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult TestToken()
        {
            var username = User.Identity?.Name;
            return Ok(new { Message = $"Token is completely valid! Welcome to the secure area, {username}." });
        }
    }
}