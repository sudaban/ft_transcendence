using Backend.Application.Abstractions;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace Backend.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new UnAuthorizedAccessException("No logged-in user found.");
            }
            return user.GetCurrentUserId();
        }
    }

    public int? MaybeUserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }

            try
            {
                return user.GetCurrentUserId();
            }
            catch
            {
                return null;
            }
        }
    }

    public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsAdmin() ?? false;

    public void CheckIfAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            throw new UnAuthorizedAccessException("No logged-in user found.");
        }
        user.CheckIfAdmin();
    }
}
