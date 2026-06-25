using AutoMapper;
using Backend.Application.Profiles;
using Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using Backend.Domain.Repositories;
using Backend.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// connection string for psql
var con_str = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(con_str, npgsqlOptions => 
        npgsqlOptions.MigrationsAssembly("Backend.Persistence"))
);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

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

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => new { message = "Transendence Backend API" });
app.MapGet("/health", () => new { status = "Backend healthy" });

app.Run();
