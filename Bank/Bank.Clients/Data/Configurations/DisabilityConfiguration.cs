using Bank.Clients.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Clients.Data.Configurations;

public sealed class DisabilityConfiguration : IEntityTypeConfiguration<Disability>
{
    public void Configure(EntityTypeBuilder<Disability> builder)
    {
        builder.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameRu).HasMaxLength(100).IsRequired();

        builder.HasData(
            new Disability { Id = 1, NameEn = "None", NameRu = "Нет" },
            new Disability { Id = 2, NameEn = "Group I", NameRu = "I группа" },
            new Disability { Id = 3, NameEn = "Group II", NameRu = "II группа" },
            new Disability { Id = 4, NameEn = "Group III", NameRu = "III группа" });
    }
}
