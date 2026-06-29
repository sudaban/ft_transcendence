using Backend.Application.DTOs.Requests.Auth;

namespace Backend.Application.Abstractions
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestDto request);
        Task<string> LoginAsync(LoginRequestDto request);
    }
}