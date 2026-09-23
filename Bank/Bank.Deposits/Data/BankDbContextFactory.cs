using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bank.Deposits.Data;

public sealed class DepositsDbContextFactory : IDesignTimeDbContextFactory<DepositsDbContext>
{
    public DepositsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DepositsDbContext>()
            .UseSqlite("Data Source=deposits.design.db")
            .Options;

        return new DepositsDbContext(options);
    }
}
