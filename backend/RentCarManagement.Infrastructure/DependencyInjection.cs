using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentCarManagement.Domain.Entities;
using RentCarManagement.Infrastructure.Data;
using RentCarManagement.Infrastructure.Services;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Services;
using RentCarManagement.Infrastructure.Data.Repositories;

namespace RentCarManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
            services.AddDbContext<RentCarDbContext>(options => options.UseInMemoryDatabase("RentCarApiIntegrationTests"));
        else
            services.AddDbContext<RentCarDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(RentCarDbContext).Assembly.FullName)));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPasswordResetNotifier, PasswordResetNotifier>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IDamageRepository, DamageRepository>();
        services.AddScoped<IApplicationSettings, ApplicationSettings>();
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        services.AddScoped<IProfileApplicationService, ProfileApplicationService>();
        services.AddScoped<IContactApplicationService, ContactApplicationService>();
        services.AddScoped<IVehicleApplicationService, VehicleApplicationService>();
        services.AddScoped<IRentalApplicationService, RentalApplicationService>();
        services.AddScoped<IDamageApplicationService, DamageApplicationService>();
        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RentCarDbContext>();
        if (environment.IsEnvironment("Testing"))
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.MigrateAsync();

        if (await db.Vehicles.AnyAsync()) return;

        db.Vehicles.AddRange(
            new Vehicle { Brand = "Toyota", Model = "Corolla", Year = 2023, Mileage = 18500, DailyRate = 1800, ExtraKilometerRate = 4, MaintenanceMileageThreshold = 30000 },
            new Vehicle { Brand = "Renault", Model = "Clio", Year = 2022, Mileage = 24200, DailyRate = 1450, ExtraKilometerRate = 3, MaintenanceMileageThreshold = 30000 },
            new Vehicle { Brand = "Fiat", Model = "Egea", Year = 2024, Mileage = 9200, DailyRate = 1600, ExtraKilometerRate = 3.5m, MaintenanceMileageThreshold = 20000 });
        await db.SaveChangesAsync();
    }
}
