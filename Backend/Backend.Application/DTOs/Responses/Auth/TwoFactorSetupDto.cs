namespace Backend.Application.DTOs.Responses.Auth
{
    public record TwoFactorSetupDto
    (
        string SecretKey,
        string QrCodeUri
    );
}
