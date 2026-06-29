using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Auth;
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
        private readonly IGenericRepository<User> _userRepository;

        public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IGenericRepository<User> userRepository)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _userRepository = userRepository;
        }

        public async Task<string> RegisterAsync(RegisterRequestDto request)
        {
            var users = await _userRepository.GetAllAsync();
            if (users.Any(u => u.Username == request.Username || u.Email == request.Email))
                throw new OverlapException("This username or email is already taken.");

            CreatePasswordHash(request.Password, out string hash, out string salt);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<string> LoginAsync(LoginRequestDto request)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                throw new NotFoundException("Invalid email or password.");

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnAuthorizedAccessException("Invalid email or password.");

            return _tokenService.CreateToken(user);
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