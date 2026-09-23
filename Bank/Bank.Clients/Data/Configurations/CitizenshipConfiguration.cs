using Bank.Clients.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Clients.Data.Configurations;

public sealed class CitizenshipConfiguration : IEntityTypeConfiguration<Citizenship>
{
    public void Configure(EntityTypeBuilder<Citizenship> builder)
    {
        builder.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameRu).HasMaxLength(100).IsRequired();

        builder.HasData(
            new Citizenship { Id = 1, NameEn = "Belarus", NameRu = "Беларусь" },
            new Citizenship { Id = 2, NameEn = "Russia", NameRu = "Россия" },
            new Citizenship { Id = 3, NameEn = "Kazakhstan", NameRu = "Казахстан" },
            new Citizenship { Id = 4, NameEn = "Other", NameRu = "Другое" });
    }
}
