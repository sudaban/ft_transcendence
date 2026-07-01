namespace Backend.Application.DTOs.Requests.Auth
{
    public record TwoFactorLoginRequestDto
    (
        string Email,
        string Code,
        string TempToken
    );
}
