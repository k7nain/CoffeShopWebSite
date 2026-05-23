using CoffeeShop.API.Configuration;
using CoffeeShop.API.Middleware;
using CoffeeShop.Application;
using CoffeeShop.Application.Configuration;
using CoffeeShop.Infrastructure;
using CoffeeShop.Infrastructure.Configuration;
using CoffeeShop.Infrastructure.Persistence;
using CoffeeShop.Infrastructure.Seeding;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings section is missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 characters for HMAC-SHA256.");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CoffeeShop.Application.Validators.Auth.RegisterRequestValidator>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer();

builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerConfiguration>();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoffeeShop API",
        Version = "v1",
        Description = "Coffee shop management system API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste only the JWT token value (do not include the word Bearer)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        var origins = builder.Configuration["CORS_ORIGINS"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        if (origins.Length == 0)
        {
            origins =
            [
                "http://localhost:5173",
                "http://localhost:3000",
                "https://localhost:5173"
            ];
        }

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<CoffeeShop.Application.Interfaces.IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var adminSetup = app.Configuration.GetSection(AdminSetupSettings.SectionName).Get<AdminSetupSettings>()
        ?? new AdminSetupSettings();

    var isInMemory = string.Equals(
        context.Database.ProviderName,
        "Microsoft.EntityFrameworkCore.InMemory",
        StringComparison.Ordinal);

    if (!isInMemory)
    {
        await context.Database.MigrateAsync();
    }

    await DbInitializer.SeedAsync(context, passwordHasher, logger, adminSetup);

    if (isInMemory)
    {
        logger.LogWarning("Using InMemory database (Development). Set UseInMemoryDatabase=false and configure PostgreSQL for persistent storage.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex,
        "Database initialization FAILED: {Message}. Set UseInMemoryDatabase=true in appsettings.Development.json or fix PostgreSQL connection.",
        ex.Message);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoffeeShop API v1");
        options.RoutePrefix = "swagger";
    });

    app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Avoid redirecting to HTTPS when only HTTP is bound (common VS "http" profile issue).
var httpsPort = app.Configuration["ASPNETCORE_HTTPS_PORT"];
var urls = app.Configuration["ASPNETCORE_URLS"] ?? string.Empty;
var listensOnHttps = !string.IsNullOrEmpty(httpsPort)
    || urls.Contains("https://", StringComparison.OrdinalIgnoreCase);

if (!app.Environment.IsDevelopment() || listensOnHttps)
{
    app.UseHttpsRedirection();
}

app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
