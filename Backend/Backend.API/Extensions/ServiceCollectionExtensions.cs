using Backend.API.Middlewares;
using Backend.Application.Abstractions;
using Backend.Application.Profiles;
using Backend.Application.Services;
using Backend.Application.Services.Users;
using Backend.Application.Services.Posts;
using Backend.Application.Services.ChatRoom;
using Backend.Application.Services.Ai;
using Backend.API.Services;
using Backend.Infrastructure;
using Backend.Infrastructure.Services;
using Backend.Persistence;
using Backend.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var con_str = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(con_str, npgsqlOptions => 
                npgsqlOptions.MigrationsAssembly("Backend.Persistence"))
        );

        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IChatRoomService, ChatRoomService>();
        services.AddHttpClient();
        services.AddScoped<IUserBlockService, UserBlockService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ILikeService, PostLikeService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ISavedPostService, SavedPostService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IChatHubService, ChatHubService>();
        services.AddScoped<IAiService, GeminiAiService>();
        services.AddSingleton<IAiChatResponder, AiChatResponder>();

        return services;
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secret_key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? configuration["JwtOptions:SecretKey"];
        if (string.IsNullOrEmpty(secret_key))
        {
            throw new InvalidOperationException("Insecure or missing JWT SecretKey!");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key)),
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtOptions:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtOptions:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var access_token = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(access_token) && path.StartsWithSegments("/chathub"))
                    {
                        context.Token = access_token;
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/problem+json";

                    var problemDetails = new ProblemDetails
                    {
                        Status = 401,
                        Title = "Unauthorized",
                        Detail = "You are not authorized to access this resource. Please log in.",
                        Instance = context.Request.Path
                    };

                    await context.Response.WriteAsJsonAsync(problemDetails);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/problem+json";

                    var problemDetails = new ProblemDetails
                    {
                        Status = 403,
                        Title = "Forbidden",
                        Detail = "You do not have permission to access this resource.",
                        Instance = context.Request.Path
                    };

                    await context.Response.WriteAsJsonAsync(problemDetails);
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.SetIsOriginAllowed(origin => true)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status200OK;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 200;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Status = 429,
                    Title = "Too Many Requests",
                    Detail = "Rate limit exceeded. Please try again later.",
                    Instance = context.HttpContext.Request.Path
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
            };
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        return services;
    }
}
