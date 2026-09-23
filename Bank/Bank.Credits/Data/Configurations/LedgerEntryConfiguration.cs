using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Credits.Data.Configurations;

public sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.AmountByn).HasPrecision(18, 2);
        builder.Property(e => e.Rate).HasPrecision(18, 6);
        builder.Property(e => e.Operation).HasMaxLength(64).IsRequired();

        builder.HasOne(e => e.DebitAccount)
            .WithMany()
            .HasForeignKey(e => e.DebitAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreditAccount)
            .WithMany()
            .HasForeignKey(e => e.CreditAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Contract)
            .WithMany()
            .HasForeignKey(e => e.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new LedgerEntry
        {
            Id = 1,
            BookedOn = new DateOnly(2026, 9, 20),
            DebitAccountId = 2,
            CreditAccountId = 3,
            Amount = 1_000_000m,
            AmountByn = 1_000_000m,
            Rate = 1m,
            Operation = "sfrbCapital"
        });
    }
}
