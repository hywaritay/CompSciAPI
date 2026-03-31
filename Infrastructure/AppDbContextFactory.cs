using CompSci.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CompSci.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var connectionString = "Host=postgres-service-zxa9w.db.eu-east-1.onmiget.com;Database=z4neudyp;Username=eiy2wfgw;Password=cNgLti2pVraETp0wIUsPSGq3C0YUDKHI;SSL Mode=Require;Trust Server Certificate=true";
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
