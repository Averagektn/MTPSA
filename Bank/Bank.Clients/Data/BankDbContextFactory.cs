using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bank.Clients.Data;

public sealed class BankDbContextFactory : IDesignTimeDbContextFactory<BankDbContext>
{
    public BankDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseSqlite("Data Source=clients.design.db")
            .Options;

        return new BankDbContext(options);
    }
}
