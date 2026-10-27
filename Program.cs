using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;
using RentCarManagement.Api.Services;
using Serilog;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo.Console().WriteTo.File("logs/rent-car-.log", rollingInterval: RollingInterval.Day));
    builder.Services.AddDbContext<RentCarDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"], ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1) });
    builder.Services.AddAuthorization();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IRentalService, RentalService>();
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles); builder.Services.AddEndpointsApiExplorer(); builder.Services.AddOpenApi();
    var app = builder.Build();
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<RentCarDbContext>();
        await db.Database.MigrateAsync();
        if (!await db.Vehicles.AnyAsync())
        {
            db.Vehicles.AddRange(
                new Vehicle { Brand = "Toyota", Model = "Corolla", Year = 2023, Mileage = 18500, DailyRate = 1800, ExtraKilometerRate = 4, MaintenanceMileageThreshold = 30000 },
                new Vehicle { Brand = "Renault", Model = "Clio", Year = 2022, Mileage = 24200, DailyRate = 1450, ExtraKilometerRate = 3, MaintenanceMileageThreshold = 30000 },
                new Vehicle { Brand = "Fiat", Model = "Egea", Year = 2024, Mileage = 9200, DailyRate = 1600, ExtraKilometerRate = 3.5m, MaintenanceMileageThreshold = 20000 });
            await db.SaveChangesAsync();
        }
    }
    app.UseSerilogRequestLogging();
    if (app.Environment.IsDevelopment()) app.MapOpenApi();
    app.UseExceptionHandler("/error");
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "Application terminated unexpectedly"); }
finally { Log.CloseAndFlush(); }
public partial class Program { }
