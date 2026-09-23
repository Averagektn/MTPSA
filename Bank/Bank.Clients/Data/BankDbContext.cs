using Bank.Clients.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Data;

public sealed class BankDbContext(DbContextOptions<BankDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<MaritalStatus> MaritalStatuses => Set<MaritalStatus>();
    public DbSet<Citizenship> Citizenships => Set<Citizenship>();
    public DbSet<Disability> Disabilities => Set<Disability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankDbContext).Assembly);
    }
}
