using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Credits.Data;

public sealed class CreditsDbContext(DbContextOptions<CreditsDbContext> options) : DbContext(options)
{
    public DbSet<ChartAccount> ChartAccounts => Set<ChartAccount>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CreditProduct> CreditProducts => Set<CreditProduct>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<CreditContract> CreditContracts => Set<CreditContract>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<BankState> BankStates => Set<BankState>();
    public DbSet<BankCard> BankCards => Set<BankCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreditsDbContext).Assembly);
    }
}
