using Backend.API.Middlewares;
using Backend.API.Extensions;
using Backend.Application.Profiles;
using Backend.Application;
using Backend.API.Hubs;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.VerifyEnvironmentVariables();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureOptions<ConfigureApiBehaviorOptions>();

builder.Services.AddCustomServices();

builder.Services.AddApplicationServices();

builder.Services.AddCustomAuthentication(builder.Configuration);

builder.Services.AddAutoMapper(cfg => {}, typeof(MappingProfile).Assembly);

builder.Services.AddCustomCors();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddCustomRateLimiter();

var app = builder.Build();

app.SeedDatabase();

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
