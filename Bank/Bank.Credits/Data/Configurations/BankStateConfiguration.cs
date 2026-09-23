using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Credits.Data.Configurations;

public sealed class BankStateConfiguration : IEntityTypeConfiguration<BankState>
{
    public void Configure(EntityTypeBuilder<BankState> builder)
    {
        builder.HasData(new BankState { Id = 1, CurrentDate = new DateOnly(2026, 9, 20) });
    }
}
