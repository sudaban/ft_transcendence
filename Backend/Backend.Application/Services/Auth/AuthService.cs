using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.DTOs.Responses.Auth;
using Backend.Application.Exceptions;
using Backend.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            ITwoFactorService twoFactorService,
            IGenericRepository<User> userRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _twoFactorService = twoFactorService;
            _userRepository = userRepository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterRequestDto request)
        {
            if (await _userRepository.TableNoTracking.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
                throw new OverlapException("This username or email is already taken.");

            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = string.Empty,
                Role = Backend.Domain.Enums.UserRole.User
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.Table.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnAuthorizedAccessException("Invalid email or password.");

            if (user.IsBanned)
                throw new UnAuthorizedAccessException("Your account has been banned.");

            if (!string.IsNullOrEmpty(user.PasswordSalt) && _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                if (_passwordHasher.NeedsRehash(user.PasswordHash))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(request.Password);
                    user.PasswordSalt = string.Empty;
                    await _userRepository.UpdateAsync(user);
                    await _unitOfWork.CommitAsync();
                }
            }
            else if (_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                if (_passwordHasher.NeedsRehash(user.PasswordHash))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(request.Password);
                    user.PasswordSalt = string.Empty;
                    await _userRepository.UpdateAsync(user);
                    await _unitOfWork.CommitAsync();
                }
            }
            else
            {
                throw new UnAuthorizedAccessException("Invalid email or password.");
            }

            if (user.IsTwoFactorEnabled)
            {
                var temp_token = _tokenService.CreateTempToken(user);
                return new LoginResponseDto(true, null, null, temp_token, user.Email);
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<LoginResponseDto> Verify2FaLoginAsync(TwoFactorLoginRequestDto request)
        {
            if (!_tokenService.ValidateTempToken(request.TempToken, out string email))
                throw new UnAuthorizedAccessException("Invalid or expired 2FA session.");

            if (email != request.Email)
                throw new UnAuthorizedAccessException("Invalid 2FA session.");

            var user = await _userRepository.Table.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new NotFoundException("User not found.");

            if (!user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                throw new BadRequestException("Two-factor authentication is not enabled for this user.");

            if (!_twoFactorService.VerifyCode(user.TwoFactorSecret, request.Code))
                throw new UnAuthorizedAccessException("Invalid verification code.");

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<TwoFactorSetupDto> SetupTwoFactorAsync(int user_id)
        {
            var user = await _userRepository.GetByIdAsync(user_id);
            if (user == null)
                throw new NotFoundException("User not found.");

            var secret = _twoFactorService.GenerateSecretKey();
            user.TwoFactorSecret = secret;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            var qr_uri = _twoFactorService.GenerateQrCodeUri(user.Email, secret);
            return new TwoFactorSetupDto(secret, qr_uri);
        }

        public async Task<bool> EnableTwoFactorAsync(int user_id, string code)
        {
            var user = await _userRepository.GetByIdAsync(user_id);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (string.IsNullOrEmpty(user.TwoFactorSecret))
                throw new BadRequestException("Two-factor authentication setup is not initiated.");

            if (!_twoFactorService.VerifyCode(user.TwoFactorSecret, code))
                return false;

            user.IsTwoFactorEnabled = true;
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DisableTwoFactorAsync(int user_id, DisableTwoFactorRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(user_id);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (!user.IsTwoFactorEnabled)
                throw new BadRequestException("Two-factor authentication is not enabled.");

            bool isVerified = false;

            if (!string.IsNullOrEmpty(request.Password))
            {
                if (!string.IsNullOrEmpty(user.PasswordSalt))
                {
                    isVerified = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
                }
                else
                {
                    isVerified = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
                }
            }
            else if (!string.IsNullOrEmpty(request.Code) && !string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                isVerified = _twoFactorService.VerifyCode(user.TwoFactorSecret, request.Code);
            }

            if (!isVerified)
            {
                throw new UnAuthorizedAccessException("Invalid password or verification code provided to disable 2FA.");
            }

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecret = null;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<LoginResponseDto> OAuthLoginAsync(OAuthLoginRequestDto request)
        {
            var provider = request.Provider.ToLowerInvariant();
            string client_id = "";
            string client_secret = "";
            string token_endpoint = "";
            string user_info_endpoint = "";

            if (provider == "42")
            {
                client_id = _configuration["OAUTH_INTRA_CLIENT_ID"] ?? "";
                client_secret = _configuration["OAUTH_INTRA_CLIENT_SECRET"] ?? "";
                token_endpoint = "https://api.intra.42.fr/oauth/token";
                user_info_endpoint = "https://api.intra.42.fr/v2/me";
            }
            else if (provider == "google")
            {
                client_id = _configuration["OAUTH_GOOGLE_CLIENT_ID"] ?? "";
                client_secret = _configuration["OAUTH_GOOGLE_CLIENT_SECRET"] ?? "";
                token_endpoint = "https://oauth2.googleapis.com/token";
                user_info_endpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
            }
            else
            {
                throw new BadRequestException("Unsupported OAuth provider.");
            }

            if (!Uri.TryCreate(request.RedirectUri, UriKind.Absolute, out var parsedRedirectUri) ||
                (!parsedRedirectUri.AbsolutePath.EndsWith("/auth/callback", StringComparison.OrdinalIgnoreCase) &&
                 !parsedRedirectUri.AbsolutePath.EndsWith("/auth/callback/", StringComparison.OrdinalIgnoreCase)))
            {
                throw new BadRequestException("Invalid OAuth redirect URI.");
            }

            var http_client = _httpClientFactory.CreateClient();
            var dict = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "code", request.Code },
                { "redirect_uri", request.RedirectUri }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, token_endpoint)
            {
                Content = new FormUrlEncodedContent(dict)
            };

            var res = await http_client.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                throw new BadRequestException("Failed to exchange OAuth code for access token.");
            }

            var token_content = await res.Content.ReadAsStringAsync();
            using var token_doc = JsonDocument.Parse(token_content);
            var access_token = token_doc.RootElement.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(access_token))
            {
                throw new BadRequestException("Access token not found in response.");
            }

            var user_info_req = new HttpRequestMessage(HttpMethod.Get, user_info_endpoint);
            user_info_req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

            var user_info_res = await http_client.SendAsync(user_info_req);
            if (!user_info_res.IsSuccessStatusCode)
            {
                throw new BadRequestException("Failed to retrieve user info from provider.");
            }

            var user_info_content = await user_info_res.Content.ReadAsStringAsync();
            using var user_info_doc = JsonDocument.Parse(user_info_content);
            string email = "";
            string username = "";

            if (provider == "42")
            {
                email = user_info_doc.RootElement.GetProperty("email").GetString() ?? "";
                username = user_info_doc.RootElement.GetProperty("login").GetString() ?? "";
            }
            else if (provider == "google")
            {
                if (user_info_doc.RootElement.TryGetProperty("email_verified", out var verifiedProp))
                {
                    bool isVerified = verifiedProp.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.String => bool.TryParse(verifiedProp.GetString(), out var parsed) && parsed,
                        _ => false
                    };

                    if (!isVerified)
                    {
                        throw new BadRequestException("OAuth email is not verified by provider.");
                    }
                }

                email = user_info_doc.RootElement.GetProperty("email").GetString() ?? "";
                username = user_info_doc.RootElement.TryGetProperty("name", out var name_prop) ? name_prop.GetString() ?? "" : email.Split('@')[0];
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new BadRequestException("Email not found in user info.");
            }

            var user = await _userRepository.Table.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                var passwordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString("N"));

                string base_username = username.Replace(" ", "_").ToLower();
                base_username = new string(base_username.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
                
                if (string.IsNullOrEmpty(base_username))
                {
                    base_username = email.Split('@')[0];
                }

                string final_username = base_username;
                int counter = 1;
                while (await _userRepository.TableNoTracking.AnyAsync(u => u.Username.ToLower() == final_username.ToLower()))
                {
                    final_username = $"{base_username}{counter}";
                    counter++;
                }

                user = new User
                {
                    Username = final_username,
                    Email = email,
                    PasswordHash = passwordHash,
                    PasswordSalt = string.Empty
                };

                await _userRepository.AddAsync(user);
                await _unitOfWork.CommitAsync();
            }

            if (user.IsBanned)
            {
                throw new UnAuthorizedAccessException("Your account has been banned.");
            }

            if (user.IsTwoFactorEnabled)
            {
                var temp_token = _tokenService.CreateTempToken(user);
                return new LoginResponseDto(true, null, null, temp_token, user.Email);
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(TokenRequestDto request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                throw new UnAuthorizedAccessException("Invalid token.");
            }

            var user_id_string = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(user_id_string) || !int.TryParse(user_id_string, out var user_id))
            {
                throw new UnAuthorizedAccessException("Invalid token.");
            }

            var user = await _userRepository.GetByIdAsync(user_id);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnAuthorizedAccessException("Invalid refresh token.");
            }

            return await GenerateAuthResponseAsync(user);
        }

        private async Task<LoginResponseDto> GenerateAuthResponseAsync(User user)
        {
            var token = _tokenService.CreateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return new LoginResponseDto(false, token, refreshToken, null);
        }
    }
}