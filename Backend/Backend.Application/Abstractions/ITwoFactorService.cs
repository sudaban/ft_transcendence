namespace Backend.Application.Abstractions
{
    public interface ITwoFactorService
    {
        string GenerateSecretKey();
        string GenerateQrCodeUri(string email, string secret_key);
        bool VerifyCode(string secret_key, string code);
    }
}
