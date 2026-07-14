using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Infrastructure
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var secret_key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? _configuration["JwtOptions:SecretKey"];
            if (string.IsNullOrEmpty(secret_key) || secret_key == "YOUR_SECRET_KEY_PLACEHOLDER_DO_NOT_COMMIT")
                throw new InvalidOperationException("Insecure or missing JWT SecretKey! Make sure JWT_SECRET_KEY is set in your environment variables (.env).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token_options = new JwtSecurityToken(
                issuer: _configuration["JwtOptions:Issuer"],
                audience: _configuration["JwtOptions:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtOptions:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token_options);
        }

        public string CreateTempToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("pre_auth", "true")
            };

            var secret_key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? _configuration["JwtOptions:SecretKey"];
            if (string.IsNullOrEmpty(secret_key) || secret_key == "YOUR_SECRET_KEY_PLACEHOLDER_DO_NOT_COMMIT")
                throw new InvalidOperationException("Insecure or missing JWT SecretKey! Make sure JWT_SECRET_KEY is set in your environment variables (.env).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token_options = new JwtSecurityToken(
                issuer: _configuration["JwtOptions:Issuer"],
                audience: _configuration["JwtOptions:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token_options);
        }

        public bool ValidateTempToken(string token, out string email)
        {
            email = string.Empty;
            if (string.IsNullOrEmpty(token))
                return false;

            var token_handler = new JwtSecurityTokenHandler();
            var secret_key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? _configuration["JwtOptions:SecretKey"];
            if (string.IsNullOrEmpty(secret_key) || secret_key == "YOUR_SECRET_KEY_PLACEHOLDER_DO_NOT_COMMIT")
                throw new InvalidOperationException("Insecure or missing JWT SecretKey! Make sure JWT_SECRET_KEY is set in your environment variables (.env).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key));
            var validation_parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtOptions:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JwtOptions:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = token_handler.ValidateToken(token, validation_parameters, out SecurityToken validated_token);
                var pre_auth_claim = principal.FindFirst("pre_auth")?.Value;
                if (pre_auth_claim != "true")
                    return false;

                var email_claim = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email_claim))
                    return false;

                email = email_claim;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateRefreshToken()
        {
            var random_bytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(random_bytes);
            return Convert.ToBase64String(random_bytes);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var secret_key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? _configuration["JwtOptions:SecretKey"];
            if (string.IsNullOrEmpty(secret_key))
                throw new InvalidOperationException("JWT SecretKey is missing in appsettings.json");

            var token_validation_parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key)),
                ValidateLifetime = false
            };

            var token_handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = token_handler.ValidateToken(token, token_validation_parameters, out SecurityToken security_token);
                if (security_token is not JwtSecurityToken jwt_security_token || 
                    !jwt_security_token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}