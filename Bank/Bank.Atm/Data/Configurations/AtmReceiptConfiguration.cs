using Bank.Atm.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Atm.Data.Configurations;

public sealed class AtmReceiptConfiguration : IEntityTypeConfiguration<AtmReceipt>
{
    public void Configure(EntityTypeBuilder<AtmReceipt> builder)
    {
        builder.Property(r => r.BodyJson).IsRequired();
        builder.HasIndex(r => r.SessionId);
    }
}
