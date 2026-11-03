using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using RentCarManagement.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, logger) => logger
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/rent-car-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
    ConfigureAuthentication(builder.Services, builder.Configuration);
    ConfigureRateLimiting(builder.Services);
    builder.Services.AddAuthorization();
    builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    var app = builder.Build();
    await app.Services.InitializeDatabaseAsync(app.Environment);

    app.UseSerilogRequestLogging();
    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "RentCar API v1"));
    }
    app.UseExceptionHandler("/error");
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtKey = configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
        throw new InvalidOperationException("Jwt:Key must be configured as a secret with at least 32 bytes.");

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        });
}

static void ConfigureRateLimiting(IServiceCollection services)
{
    services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
        options.AddPolicy("contact", context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
    });
}

public partial class Program;
