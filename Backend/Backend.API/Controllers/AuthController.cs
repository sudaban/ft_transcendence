using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var token = await _authService.LoginAsync(request);
            return Ok(new { Message = "Login successful", Token = token });
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