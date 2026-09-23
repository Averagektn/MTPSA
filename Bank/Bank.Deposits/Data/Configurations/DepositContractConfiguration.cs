using Bank.Deposits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Deposits.Data.Configurations;

public sealed class DepositContractConfiguration : IEntityTypeConfiguration<DepositContract>
{
    public void Configure(EntityTypeBuilder<DepositContract> builder)
    {
        builder.Property(c => c.Number).HasMaxLength(32).IsRequired();
        builder.Property(c => c.ClientName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Amount).HasPrecision(18, 2);
        builder.Property(c => c.AnnualRate).HasPrecision(18, 4);
        builder.HasIndex(c => c.Number).IsUnique();

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Currency)
            .WithMany()
            .HasForeignKey(c => c.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PrincipalAccount)
            .WithMany()
            .HasForeignKey(c => c.PrincipalAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.InterestAccount)
            .WithMany()
            .HasForeignKey(c => c.InterestAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
