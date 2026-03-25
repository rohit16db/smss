using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using SMS.API.Extensions;
using SMS.API.Filters;
using SMS.API.Middleware;
using SMS.API.Services;
using SMS.Application.Common.Behaviors;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.Services;
using SMS.Domain.Interfaces;
using SMS.Infrastructure.Data;
using SMS.Infrastructure.Data.Interceptors;
using SMS.Infrastructure.Services;

// Define role constants
const string RoleAdmin = "Admin";
const string RoleAccountant = "Accountant";
const string RoleClerk = "Clerk";
const string RoleTeacher = "Teacher";

var builder = WebApplication.CreateBuilder(args);

// Configure QuestPDF License (Community License - free for open-source)
QuestPDF.Settings.License = LicenseType.Community;

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
    })
    .AddInterceptors(new UtcDateTimeInterceptor())
    .ConfigureWarnings(warnings => 
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Register DbContext interface
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Register custom services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IStudentIdGenerator, StudentIdGenerator>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IRollNumberService, RollNumberService>();
builder.Services.AddScoped<IAcademicYearContext, AcademicYearContext>();

// Register Exam Module Domain Services (SRP: Single Responsibility Principle)
builder.Services.AddScoped<IGradeCalculationService, GradeCalculationService>();
builder.Services.AddScoped<IMarksCalculationService, MarksCalculationService>();
builder.Services.AddScoped<IClassPositionService, ClassPositionService>();

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
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role // Explicitly map role claim type
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(RoleAdmin));
    options.AddPolicy("AcademicAccess", policy =>
        policy.RequireRole(RoleAdmin, RoleClerk));
    options.AddPolicy("FeesAccess", policy =>
        policy.RequireRole(RoleAdmin, RoleAccountant));
    options.AddPolicy("SalaryAccess", policy =>
        policy.RequireRole(RoleAdmin, RoleAccountant, RoleTeacher));
    options.AddPolicy("PayrollAccess", policy =>
        policy.RequireRole(RoleAdmin, RoleAccountant));
    options.AddPolicy("SalaryManageAccess", policy =>
        policy.RequireRole(RoleAdmin, RoleAccountant));
    options.AddPolicy("AttendanceAccess", policy =>
        policy.RequireRole(RoleAdmin, RoleClerk, RoleTeacher));
    options.AddPolicy("DashboardAccess", policy =>
        policy.RequireRole(RoleAdmin, RoleAccountant, RoleClerk));
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
app.UseMiddleware<AcademicYearMiddleware>();

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
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        await next();
    });
}

app.UseHttpsRedirection();

// Enable static files for image uploads
app.UseStaticFiles();

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

await app.RunAsync();
