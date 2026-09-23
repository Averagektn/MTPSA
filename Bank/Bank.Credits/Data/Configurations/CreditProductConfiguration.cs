using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Credits.Data.Configurations;

public sealed class CreditProductConfiguration : IEntityTypeConfiguration<CreditProduct>
{
    public void Configure(EntityTypeBuilder<CreditProduct> builder)
    {
        builder.Property(p => p.Code).HasMaxLength(32).IsRequired();
        builder.Property(p => p.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NameRu).HasMaxLength(200).IsRequired();
        builder.Property(p => p.AnnualRate).HasPrecision(18, 4);
        builder.Property(p => p.MinAmount).HasPrecision(18, 2);
        builder.Property(p => p.MaxAmount).HasPrecision(18, 2);
        builder.HasIndex(p => p.Code).IsUnique();

        builder.HasOne(p => p.Currency)
            .WithMany()
            .HasForeignKey(p => p.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PrincipalChartAccount)
            .WithMany()
            .HasForeignKey(p => p.PrincipalChartAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.InterestChartAccount)
            .WithMany()
            .HasForeignKey(p => p.InterestChartAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new CreditProduct
            {
                Id = 1,
                Code = "ALFA_CASH",
                NameEn = "Cash loan (annuity)",
                NameRu = "Кредит наличными (аннуитет)",
                RepaymentSchedule = RepaymentSchedule.Annuity,
                TermMonths = 12,
                AnnualRate = 18.1m,
                MinAmount = 1000m,
                MaxAmount = 20_000m,
                CurrencyId = 1,
                PrincipalChartAccountId = 3,
                InterestChartAccountId = 5
            },
            new CreditProduct
            {
                Id = 2,
                Code = "ALFA_ONLINE",
                NameEn = "Online loan (interest monthly, principal at term end)",
                NameRu = "Кредит онлайн (проценты ежемесячно, тело в конце срока)",
                RepaymentSchedule = RepaymentSchedule.InterestOnly,
                TermMonths = 24,
                AnnualRate = 18.1m,
                MinAmount = 500m,
                MaxAmount = 7000m,
                CurrencyId = 1,
                PrincipalChartAccountId = 4,
                InterestChartAccountId = 6
            });
    }
}
