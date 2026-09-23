using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bank.Credits.Data;

public sealed class CreditsDbContextFactory : IDesignTimeDbContextFactory<CreditsDbContext>
{
    public CreditsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CreditsDbContext>()
            .UseSqlite("Data Source=credits.design.db")
            .Options;

        return new CreditsDbContext(options);
    }
}
