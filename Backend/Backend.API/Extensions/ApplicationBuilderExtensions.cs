using Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Backend.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void VerifyEnvironmentVariables(this WebApplicationBuilder builder)
    {
        var secret_key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["JwtOptions:SecretKey"];
        if (string.IsNullOrEmpty(secret_key) || secret_key == "YOUR_SECRET_KEY_PLACEHOLDER_DO_NOT_COMMIT")
        {
            throw new InvalidOperationException("Insecure or missing JWT SecretKey! Make sure JWT_SECRET_KEY is set in your environment variables (.env).");
        }

        var admin_email_env = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var admin_password_env = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if (string.IsNullOrEmpty(admin_email_env) || string.IsNullOrEmpty(admin_password_env))
        {
            throw new InvalidOperationException("ADMIN_EMAIL or ADMIN_PASSWORD environment variable is missing! Please configure them in your .env file.");
        }

        var http_port_env = Environment.GetEnvironmentVariable("HTTP_PORT");
        var https_port_env = Environment.GetEnvironmentVariable("HTTPS_PORT");
        if (string.IsNullOrEmpty(http_port_env) || string.IsNullOrEmpty(https_port_env))
        {
            throw new InvalidOperationException("HTTP_PORT or HTTPS_PORT environment variable is missing! Please configure them in your .env file.");
        }
    }

    public static void SeedDatabase(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
            {
                var adminExists = context.Users.Any(u => u.Email == adminEmail);
                if (!adminExists)
                {
                    using var hmac = new HMACSHA512();
                    var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(adminPassword)));
                    var passwordSalt = Convert.ToBase64String(hmac.Key);

                    var adminUser = new Backend.Domain.Entities.User
                    {
                        Username = "admin",
                        Email = adminEmail,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        Role = Backend.Domain.Enums.UserRole.Admin,
                        IsTosAccepted = true,
                        TosAcceptedAt = DateTime.UtcNow
                    };
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }
            }

            var aiExists = context.Users.Any(u => u.IsAiAssistant);
            if (!aiExists)
            {
                using var aiHmac = new HMACSHA512();
                var aiPassword = Guid.NewGuid().ToString("N");
                var aiPasswordHash = Convert.ToBase64String(aiHmac.ComputeHash(Encoding.UTF8.GetBytes(aiPassword)));
                var aiPasswordSalt = Convert.ToBase64String(aiHmac.Key);

                var aiUser = new Backend.Domain.Entities.User
                {
                    Username = "ai_assistant",
                    FullName = "AI Assistant",
                    Email = "ai@transcendence.local",
                    Bio = "🤖 I am the platform's AI assistant. Send me a message and let's chat!",
                    PasswordHash = aiPasswordHash,
                    PasswordSalt = aiPasswordSalt,
                    Role = Backend.Domain.Enums.UserRole.User,
                    IsAiAssistant = true,
                    IsTosAccepted = true,
                    TosAcceptedAt = DateTime.UtcNow,
                    IsOnline = true
                };
                context.Users.Add(aiUser);
                context.SaveChanges();
            }
        }
    }
}
