using Bank.Clients.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Clients.Data.Configurations;

public sealed class MaritalStatusConfiguration : IEntityTypeConfiguration<MaritalStatus>
{
    public void Configure(EntityTypeBuilder<MaritalStatus> builder)
    {
        builder.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameRu).HasMaxLength(100).IsRequired();

        builder.HasData(
            new MaritalStatus { Id = 1, NameEn = "Single", NameRu = "Холост / не замужем" },
            new MaritalStatus { Id = 2, NameEn = "Married", NameRu = "Женат / замужем" },
            new MaritalStatus { Id = 3, NameEn = "Divorced", NameRu = "Разведён(а)" },
            new MaritalStatus { Id = 4, NameEn = "Widowed", NameRu = "Вдовец / вдова" });
    }
}
