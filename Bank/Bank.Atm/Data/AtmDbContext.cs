using Bank.Atm.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Atm.Data;

public sealed class AtmDbContext(DbContextOptions<AtmDbContext> options) : DbContext(options)
{
    public DbSet<AtmSession> Sessions => Set<AtmSession>();
    public DbSet<AtmReceipt> Receipts => Set<AtmReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtmDbContext).Assembly);
    }
}
