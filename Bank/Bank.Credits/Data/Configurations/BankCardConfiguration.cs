using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Credits.Data.Configurations;

public sealed class BankCardConfiguration : IEntityTypeConfiguration<BankCard>
{
    public void Configure(EntityTypeBuilder<BankCard> builder)
    {
        builder.Property(c => c.CardNumber).HasMaxLength(16).IsRequired();
        builder.Property(c => c.Pin).HasMaxLength(4).IsRequired();
        builder.HasIndex(c => c.CardNumber).IsUnique();

        builder.HasOne(c => c.CreditContract)
            .WithMany()
            .HasForeignKey(c => c.CreditContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CardAccount)
            .WithMany()
            .HasForeignKey(c => c.CardAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
