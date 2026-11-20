using System.Text;
using System.Threading.RateLimiting;
using KiteSocial.API.Data;
using KiteSocial.API.Infrastructure;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services;
using KiteSocial.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Global Exception Handling & RFC 7807 ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health Checks (Liveness and Readiness probes for Docker / Cloud)
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<KiteSocialDbContext>("database", tags: new[] { "ready" });

// Database context (Flexible support for SQLite and PostgreSQL)
var dbProvider = Environment.GetEnvironmentVariable("DATABASE_PROVIDER")
    ?? builder.Configuration["DatabaseProvider"] 
    ?? "Sqlite";

builder.Services.AddDbContext<KiteSocialDbContext>(options =>
{
    if (dbProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        var connStr = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? builder.Configuration.GetConnectionString("PostgreSql") 
            ?? builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connStr);
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// ASP.NET Core Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;

        // Account lockout against brute-force attacks
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<KiteSocialDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication configuration (prioritizing Environment Variables for production)
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Critical Security Failure: JWT Secret Key is missing. Set 'JWT_SECRET_KEY' environment variable.");
}
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

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
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Rate Limiting protection (DoS and brute-force mitigation)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Strict rate limiter for authentication endpoints (5 requests/minute per client)
    options.AddFixedWindowLimiter("auth-limit", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // General rate limiter for standard API endpoints (120 requests/minute per client)
    options.AddSlidingWindowLimiter("api-limit", opt =>
    {
        opt.PermitLimit = 120;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 4;
        opt.QueueLimit = 0;
    });
});

// CORS policy for Flutter client (Web, Mobile, Desktop)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterClientPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Application Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISpotService, SpotService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IPostService, PostService>();

var app = builder.Build();

// Auto-apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KiteSocialDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FlutterClientPolicy");

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Health Check Endpoints
app.MapHealthChecks("/healthz").AllowAnonymous();
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

app.MapControllers().RequireRateLimiting("api-limit");

app.Run();
