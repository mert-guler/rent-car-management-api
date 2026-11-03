using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RentCarManagement.Infrastructure.Data;

public sealed class RentCarDbContextFactory : IDesignTimeDbContextFactory<RentCarDbContext>
{
    public RentCarDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RentCarDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=RentCarManagementDesignTime;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new RentCarDbContext(options);
    }
}
