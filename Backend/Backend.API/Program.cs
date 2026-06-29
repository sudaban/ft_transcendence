using Backend.API.Middlewares;
using Backend.Application.Abstractions;
using Backend.Application.Profiles;
using Backend.Application.Services;
using Backend.Infrastructure;
using Backend.Persistence;
using Backend.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// connection string for psql
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
builder.Services.AddScoped<IAuthService, AuthService>();

//AutoMapper Konfigürasyonu
builder.Services.AddAutoMapper(cfg =>
{
    cfg.DisableConstructorMapping();
}, typeof(MappingProfile).Assembly);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.UseExceptionHandler();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => new { status = "Backend healthy" });

app.Run();
