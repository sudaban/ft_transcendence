using Backend.Application.Exceptions;
using Backend.Domain.Enums;
using System.Security.Claims;

namespace Backend.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetCurrentUserId(this ClaimsPrincipal principal)
    {
        var user_id_string = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(user_id_string) || !int.TryParse(user_id_string, out var user_id))
        {
            throw new UnAuthorizedAccessException("No logged-in user found.");
        }
        return user_id;
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
        var role_string = principal?.FindFirst(ClaimTypes.Role)?.Value;
        return role_string == UserRole.Admin.ToString();
    }
}
