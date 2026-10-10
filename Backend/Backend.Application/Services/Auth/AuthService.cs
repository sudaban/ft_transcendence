using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.DTOs.Responses.Auth;
using Backend.Application.Exceptions;
using Backend.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IOAuthService _oauthService;

        public AuthService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            ITwoFactorService twoFactorService,
            IGenericRepository<User> userRepository,
            IOAuthService oauthService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _twoFactorService = twoFactorService;
            _userRepository = userRepository;
            _oauthService = oauthService;
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
            var userInfo = await _oauthService.GetOAuthUserInfoAsync(request.Provider, request.Code, request.RedirectUri);
            var email = userInfo.Email;
            var username = userInfo.Username;

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
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnAuthorizedAccessException("Invalid refresh token.");
            }

            var hashedRequestToken = HashRefreshToken(request.RefreshToken);
            bool isValid = user.RefreshToken == hashedRequestToken || user.RefreshToken == request.RefreshToken;
            if (!isValid)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.CommitAsync();
                throw new UnAuthorizedAccessException("Invalid refresh token. Token reuse detected.");
            }

            return await GenerateAuthResponseAsync(user);
        }

        private async Task<LoginResponseDto> GenerateAuthResponseAsync(User user)
        {
            var token = _tokenService.CreateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = HashRefreshToken(refreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return new LoginResponseDto(false, token, refreshToken, null);
        }

        private static string HashRefreshToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);
        }
    }
}