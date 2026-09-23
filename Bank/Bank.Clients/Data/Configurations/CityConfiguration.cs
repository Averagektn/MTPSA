using Bank.Clients.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Clients.Data.Configurations;

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameRu).HasMaxLength(100).IsRequired();

        builder.HasData(
            new City { Id = 1, NameEn = "Minsk", NameRu = "Минск" },
            new City { Id = 2, NameEn = "Brest", NameRu = "Брест" },
            new City { Id = 3, NameEn = "Grodno", NameRu = "Гродно" },
            new City { Id = 4, NameEn = "Vitebsk", NameRu = "Витебск" },
            new City { Id = 5, NameEn = "Mogilev", NameRu = "Могилёв" },
            new City { Id = 6, NameEn = "Gomel", NameRu = "Гомель" });
    }
}
