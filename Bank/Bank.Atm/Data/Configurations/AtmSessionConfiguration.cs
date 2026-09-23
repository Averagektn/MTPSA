using Bank.Atm.Models;
using Bank.Common.Atm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Atm.Data.Configurations;

public sealed class AtmSessionConfiguration : IEntityTypeConfiguration<AtmSession>
{
    public void Configure(EntityTypeBuilder<AtmSession> builder)
    {
        builder.Property(s => s.Screen)
            .HasConversion(screen => screen.ToWire(), value => AtmEnum.Parse<AtmScreen>(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(s => s.CardNumber).HasMaxLength(16);
        builder.Property(s => s.Pin).HasMaxLength(4);
        builder.Property(s => s.FieldsJson).IsRequired();
    }
}
