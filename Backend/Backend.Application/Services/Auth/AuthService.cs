using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.DTOs.Responses.Auth;
using Backend.Application.Exceptions;
using Backend.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IGenericRepository<User> _userRepository;

        public AuthService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ITwoFactorService twoFactorService,
            IGenericRepository<User> userRepository)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _twoFactorService = twoFactorService;
            _userRepository = userRepository;
        }

        public async Task<string> RegisterAsync(RegisterRequestDto request)
        {
            var users = await _userRepository.GetAllAsync();
            if (users.Any(u => u.Username == request.Username || u.Email == request.Email))
                throw new OverlapException("This username or email is already taken.");

            CreatePasswordHash(request.Password, out string password_hash, out string password_salt);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = password_hash,
                PasswordSalt = password_salt
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                throw new NotFoundException("Invalid email or password.");

            if (user.IsBanned)
                throw new UnAuthorizedAccessException("Your account has been banned.");

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnAuthorizedAccessException("Invalid email or password.");

            if (user.IsTwoFactorEnabled)
            {
                var temp_token = _tokenService.CreateTempToken(user);
                return new LoginResponseDto(true, null, temp_token);
            }

            var token = _tokenService.CreateToken(user);
            return new LoginResponseDto(false, token, null);
        }

        public async Task<LoginResponseDto> Verify2FaLoginAsync(TwoFactorLoginRequestDto request)
        {
            if (!_tokenService.ValidateTempToken(request.TempToken, out string email))
                throw new UnAuthorizedAccessException("Invalid or expired 2FA session.");

            if (email != request.Email)
                throw new UnAuthorizedAccessException("Invalid 2FA session.");

            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                throw new NotFoundException("User not found.");

            if (!user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                throw new InvalidOperationException("Two-factor authentication is not enabled for this user.");

            if (!_twoFactorService.VerifyCode(user.TwoFactorSecret, request.Code))
                throw new UnAuthorizedAccessException("Invalid verification code.");

            var token = _tokenService.CreateToken(user);
            return new LoginResponseDto(false, token, null);
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
                throw new InvalidOperationException("Two-factor authentication setup is not initiated.");

            if (!_twoFactorService.VerifyCode(user.TwoFactorSecret, code))
                return false;

            user.IsTwoFactorEnabled = true;
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DisableTwoFactorAsync(int user_id)
        {
            var user = await _userRepository.GetByIdAsync(user_id);
            if (user == null)
                throw new NotFoundException("User not found.");

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecret = null;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        private bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt)))
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == passwordHash;
            }
        }
    }
}