using Bank.Deposits.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Deposits.Data;

public sealed class DepositsDbContext(DbContextOptions<DepositsDbContext> options) : DbContext(options)
{
    public DbSet<ChartAccount> ChartAccounts => Set<ChartAccount>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<DepositProduct> DepositProducts => Set<DepositProduct>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<DepositContract> DepositContracts => Set<DepositContract>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<BankState> BankStates => Set<BankState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DepositsDbContext).Assembly);
    }
}
