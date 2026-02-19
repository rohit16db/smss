using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SMS.API.Extensions;
using SMS.API.Filters;
using SMS.API.Middleware;
using SMS.Application.Common.Behaviors;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Interfaces;
using SMS.Infrastructure.Data;
using SMS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Add automatic model validation filter
    options.Filters.Add<ModelStateValidationFilter>();
});

// Configure JSON serialization to use camelCase
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Configure MediatR with pipeline behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssembly(typeof(IApplicationDbContext).Assembly);

// Configure Entity Framework Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});

// Register DbContext interface
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Register custom services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IStudentIdGenerator, StudentIdGenerator>();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET not configured");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "SchoolManagementSystem";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "SMSWebClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("AcademicAccess", policy =>
        policy.RequireRole("Admin", "Clerk"));
    options.AddPolicy("FeesAccess", policy =>
        policy.RequireRole("Admin", "Accountant"));
    options.AddPolicy("SalaryAccess", policy =>
        policy.RequireRole("Admin", "Accountant", "Teacher"));
    options.AddPolicy("PayrollAccess", policy =>
        policy.RequireRole("Admin", "Accountant"));
    options.AddPolicy("SalaryManageAccess", policy =>
        policy.RequireRole("Admin", "Accountant"));
    options.AddPolicy("AttendanceAccess", policy =>
        policy.RequireRole("Admin", "Clerk", "Teacher"));
    options.AddPolicy("DashboardAccess", policy =>
        policy.RequireRole("Admin", "Accountant", "Clerk"));
});

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfigured", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // Development policy for localhost
    options.AddPolicy("Development", policy =>
    {
        policy.SetIsOriginAllowed(origin => origin.StartsWith("http://localhost"))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "School Management System API",
        Version = "v1",
        Description = "API for School Management Software with JWT Authentication"
    });
});

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgres",
        timeout: TimeSpan.FromSeconds(3),
        tags: new[] { "db", "sql", "postgres" });

var app = builder.Build();

// Add global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Add rate limiting middleware
app.UseMiddleware<RateLimitingMiddleware>();

// Configure the HTTP request pipeline
// Enable Swagger always for development
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMS API v1");
    c.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("AllowConfigured");
    // Add security headers in production
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        await next();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Run database migrations for all environments
// Seed database with initial data (development only)
await app.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    await app.SeedDatabaseAsync();
}

app.Run();
