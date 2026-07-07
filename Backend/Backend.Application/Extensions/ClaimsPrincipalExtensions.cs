using Backend.Application.Exceptions;
using Backend.Domain.Enums;
using System.Security.Claims;

namespace Backend.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetCurrentUserId(this ClaimsPrincipal principal)
    {
        var userIdString = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            throw new UnAuthorizedAccessException("No logged-in user found.");
        }
        return userId;
    }

    public static void CheckIfAdmin(this ClaimsPrincipal principal)
    {
        if (!principal.IsAdmin())
        {
            throw new UnAuthorizedAccessException("Admin privileges are required to perform this action.");
        }
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        var roleString = principal?.FindFirst(ClaimTypes.Role)?.Value;
        return roleString == UserRole.Admin.ToString();
    }
}
