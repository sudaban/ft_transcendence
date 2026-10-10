using Backend.Application.Abstractions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Backend.API.Middlewares;

public class UserBanCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserBanCheckMiddleware> _logger;

    public UserBanCheckMiddleware(RequestDelegate next, ILogger<UserBanCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IGenericRepository<User> userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var userId = context.User.GetCurrentUserId();
                var user = await userRepository.TableNoTracking
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null && user.IsBanned)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Your account has been banned.\"}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying user ban status.");
            }
        }

        await _next(context);
    }
}
