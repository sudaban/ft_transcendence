namespace Backend.Application.DTOs.Requests.Auth;

public record DisableTwoFactorRequestDto
(
    string? Password = null,
    string? Code = null
);
