using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.DTOs.Responses.Auth;

namespace Backend.Application.Abstractions
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<LoginResponseDto> Verify2FaLoginAsync(TwoFactorLoginRequestDto request);
        Task<TwoFactorSetupDto> SetupTwoFactorAsync(int user_id);
        Task<bool> EnableTwoFactorAsync(int user_id, string code);
        Task<bool> DisableTwoFactorAsync(int user_id);
        Task<LoginResponseDto> OAuthLoginAsync(OAuthLoginRequestDto request);
        Task<LoginResponseDto> RefreshTokenAsync(TokenRequestDto request);
    }
}