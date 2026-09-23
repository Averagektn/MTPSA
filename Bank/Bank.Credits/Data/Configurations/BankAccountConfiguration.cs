using Bank.Credits.Accounting;
using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Credits.Data.Configurations;

public sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.Property(a => a.Number).HasMaxLength(13).IsRequired();
        builder.Property(a => a.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(a => a.NameRu).HasMaxLength(200).IsRequired();
        builder.HasIndex(a => a.Number).IsUnique();

        builder.HasOne(a => a.ChartAccount)
            .WithMany()
            .HasForeignKey(a => a.ChartAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Currency)
            .WithMany()
            .HasForeignKey(a => a.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new BankAccount
            {
                Id = 1,
                Number = BankAccountNumbers.Cash,
                ChartAccountId = 1,
                NameEn = "Bank cash",
                NameRu = "Касса банка",
                CurrencyId = 1
            },
            new BankAccount
            {
                Id = 2,
                Number = BankAccountNumbers.Correspondent,
                ChartAccountId = 2,
                NameEn = "Correspondent account in the National Bank of Belarus",
                NameRu = "Корреспондентский счёт в НБ РБ",
                CurrencyId = 1
            },
            new BankAccount
            {
                Id = 3,
                Number = BankAccountNumbers.DevelopmentFund,
                ChartAccountId = 7,
                NameEn = "Bank development fund",
                NameRu = "Фонд развития банка",
                CurrencyId = 1
            },
            new BankAccount
            {
                Id = 4,
                Number = BankAccountNumbers.MobileSettlements,
                ChartAccountId = 9,
                NameEn = "Settlements with mobile operators",
                NameRu = "Расчёты с операторами связи",
                CurrencyId = 1
            });
    }
}
