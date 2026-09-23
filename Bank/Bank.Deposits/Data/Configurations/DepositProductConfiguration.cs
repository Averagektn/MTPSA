using Bank.Deposits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Deposits.Data.Configurations;

public sealed class DepositProductConfiguration : IEntityTypeConfiguration<DepositProduct>
{
    public void Configure(EntityTypeBuilder<DepositProduct> builder)
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
            new DepositProduct
            {
                Id = 1,
                Code = "ALFA_SAFE",
                NameEn = "Alfa Safe (revocable)",
                NameRu = "Альфа Сейф (отзывный)",
                Revocable = true,
                InterestSchedule = InterestSchedule.Monthly,
                TermMonths = 13,
                AnnualRate = 5.5m,
                MinAmount = 200m,
                MaxAmount = 20_000m,
                CurrencyId = 1,
                PrincipalChartAccountId = 4,
                InterestChartAccountId = 6
            },
            new DepositProduct
            {
                Id = 2,
                Code = "ALFA_VKLAD",
                NameEn = "Alfa Vklad (irrevocable)",
                NameRu = "Альфа Вклад (безотзывный)",
                Revocable = false,
                InterestSchedule = InterestSchedule.EndOfTerm,
                TermMonths = 13,
                AnnualRate = 12m,
                MinAmount = 50m,
                MaxAmount = 5_000_000m,
                CurrencyId = 1,
                PrincipalChartAccountId = 5,
                InterestChartAccountId = 7
            });
    }
}
