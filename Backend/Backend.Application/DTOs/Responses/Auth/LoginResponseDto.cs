namespace Backend.Application.DTOs.Responses.Auth
{
    public record LoginResponseDto
    (
        bool RequiresTwoFactor,
        string? Token,
        string? TempToken
    );
}
