using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Credits.Data.Configurations;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.Property(c => c.Code).HasMaxLength(3).IsRequired();
        builder.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameRu).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasData(
            new Currency { Id = 1, Code = "BYN", NameEn = "Belarusian ruble", NameRu = "Белорусский рубль" },
            new Currency { Id = 2, Code = "USD", NameEn = "US dollar", NameRu = "Доллар США" },
            new Currency { Id = 3, Code = "EUR", NameEn = "Euro", NameRu = "Евро" });
    }
}
