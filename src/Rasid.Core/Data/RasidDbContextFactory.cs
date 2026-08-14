using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Rasid.Core.Data;

public class RasidDbContextFactory : IDesignTimeDbContextFactory<RasidDbContext>
{
    public RasidDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<RasidDbContext> options = new DbContextOptionsBuilder<RasidDbContext>()
            .UseSqlite("Data Source=rasid-design.db")
            .Options;

        return new RasidDbContext(options);
    }
}