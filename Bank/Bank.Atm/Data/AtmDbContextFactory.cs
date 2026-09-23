using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bank.Atm.Data;

public sealed class AtmDbContextFactory : IDesignTimeDbContextFactory<AtmDbContext>
{
    public AtmDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AtmDbContext>()
            .UseSqlite("Data Source=atm.design.db")
            .Options;

        return new AtmDbContext(options);
    }
}
