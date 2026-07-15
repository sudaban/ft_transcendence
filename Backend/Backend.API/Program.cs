using Backend.API.Middlewares;
using Backend.API.Extensions;
using Backend.Application.Abstractions;
using Backend.Application.Profiles;
using Backend.Application.Services;
using Backend.Application;
using Backend.API.Hubs;
using Backend.Infrastructure;
using Backend.Persistence;
using Backend.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var con_str = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(con_str, npgsqlOptions => 
        npgsqlOptions.MigrationsAssembly("Backend.Persistence"))
);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureOptions<ConfigureApiBehaviorOptions>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITwoFactorService, Backend.Infrastructure.Services.TwoFactorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileUploadService, Backend.Infrastructure.Services.FileUploadService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, Backend.Application.Services.Users.UserService>();
builder.Services.AddScoped<IChatRoomService, Backend.Application.Services.ChatRoom.ChatRoomService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUserBlockService, Backend.Application.Services.Users.UserBlockService>();
builder.Services.AddScoped<IFollowService, Backend.Application.Services.Users.FollowService>();
builder.Services.AddScoped<IPostService, Backend.Application.Services.Posts.PostService>();
builder.Services.AddScoped<ILikeService, Backend.Application.Services.Posts.PostLikeService>();
builder.Services.AddScoped<ICommentService, Backend.Application.Services.Posts.CommentService>();
builder.Services.AddScoped<ISavedPostService, Backend.Application.Services.Posts.SavedPostService>();
builder.Services.AddScoped<IMessageService, Backend.Application.Services.ChatRoom.MessageService>();
builder.Services.AddScoped<IChatHubService, Backend.API.Services.ChatHubService>();
builder.Services.AddScoped<IAiService, Backend.Infrastructure.Services.GeminiAiService>();
builder.Services.AddSingleton<IAiChatResponder, Backend.Application.Services.Ai.AiChatResponder>();

builder.Services.AddApplicationServices();

var secret_key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["JwtOptions:SecretKey"];
if (string.IsNullOrEmpty(secret_key) || secret_key == "YOUR_SECRET_KEY_PLACEHOLDER_DO_NOT_COMMIT")
{
    throw new InvalidOperationException("Insecure or missing JWT SecretKey! Make sure JWT_SECRET_KEY is set in your environment variables (.env).");
}

var admin_email_env = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
var admin_password_env = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
if (string.IsNullOrEmpty(admin_email_env) || string.IsNullOrEmpty(admin_password_env))
{
    throw new InvalidOperationException("ADMIN_EMAIL or ADMIN_PASSWORD environment variable is missing! Please configure them in your .env file.");
}

var http_port_env = Environment.GetEnvironmentVariable("HTTP_PORT");
var https_port_env = Environment.GetEnvironmentVariable("HTTPS_PORT");
if (string.IsNullOrEmpty(http_port_env) || string.IsNullOrEmpty(https_port_env))
{
    throw new InvalidOperationException("HTTP_PORT or HTTPS_PORT environment variable is missing! Please configure them in your .env file.");
}

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = builder.Configuration["JwtOptions:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtOptions:Audience"],
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
        }
    };
});

builder.Services.AddAutoMapper(cfg => {}, typeof(MappingProfile).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                  "https://localhost",
                  $"https://localhost:{https_port_env}",
                  $"http://localhost:{http_port_env}",
                  "http://localhost:3000",
                  "https://tr.celten.fun"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
    {
        var adminExists = context.Users.Any(u => u.Email == adminEmail);
        if (!adminExists)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(adminPassword)));
            var passwordSalt = Convert.ToBase64String(hmac.Key);

            var adminUser = new Backend.Domain.Entities.User
            {
                Username = "admin",
                Email = adminEmail,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = Backend.Domain.Enums.UserRole.Admin,
                IsTosAccepted = true,
                TosAcceptedAt = DateTime.UtcNow
            };
            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }

    var aiExists = context.Users.Any(u => u.IsAiAssistant);
    if (!aiExists)
    {
        using var aiHmac = new System.Security.Cryptography.HMACSHA512();
        var aiPassword = Guid.NewGuid().ToString("N");
        var aiPasswordHash = Convert.ToBase64String(aiHmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(aiPassword)));
        var aiPasswordSalt = Convert.ToBase64String(aiHmac.Key);

        var aiUser = new Backend.Domain.Entities.User
        {
            Username = "ai_assistant",
            FullName = "AI Assistant",
            Email = "ai@transcendence.local",
            Bio = "🤖 I am the platform's AI assistant. Send me a message and let's chat!",
            PasswordHash = aiPasswordHash,
            PasswordSalt = aiPasswordSalt,
            Role = Backend.Domain.Enums.UserRole.User,
            IsAiAssistant = true,
            IsTosAccepted = true,
            TosAcceptedAt = DateTime.UtcNow,
            IsOnline = true
        };
        context.Users.Add(aiUser);
        context.SaveChanges();
    }
}

app.UseExceptionHandler();
app.UseCors("AllowAll");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserBanCheckMiddleware>();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.MapGet("/health", () => new { status = "Backend healthy" });

app.Run();
