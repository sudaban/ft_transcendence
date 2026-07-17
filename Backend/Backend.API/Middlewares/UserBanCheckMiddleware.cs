using Backend.Application.Abstractions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;

namespace Backend.API.Middlewares
{
    public class UserBanCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public UserBanCheckMiddleware(RequestDelegate next)
        {
            _next = next;
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
                catch
                {
                    
                }
            }

            await _next(context);
        }
    }
}
