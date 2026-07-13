using Backend.Domain.Entities;

namespace Backend.Application.Abstractions
{
    public interface ITokenService
    {
        string CreateToken(User user);
        string CreateTempToken(User user);
        bool ValidateTempToken(string token, out string email);
        string GenerateRefreshToken();
        System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}