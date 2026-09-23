using Bank.Deposits.Accounting;
using Bank.Deposits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Deposits.Data.Configurations;

public sealed class ChartAccountConfiguration : IEntityTypeConfiguration<ChartAccount>
{
    public void Configure(EntityTypeBuilder<ChartAccount> builder)
    {
        builder.Property(c => c.Code).HasMaxLength(4).IsRequired();
        builder.Property(c => c.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(c => c.NameRu).HasMaxLength(200).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasData(
            new ChartAccount { Id = 1, Code = ChartCodes.Cash, NameEn = "Bank cash", NameRu = "Касса банка", Nature = AccountNature.Active },
            new ChartAccount { Id = 2, Code = ChartCodes.Correspondent, NameEn = "Correspondent account in the National Bank of Belarus", NameRu = "Корреспондентский счёт в НБ РБ", Nature = AccountNature.Active },
            new ChartAccount { Id = 3, Code = ChartCodes.CurrentIndividuals, NameEn = "Current accounts of individuals", NameRu = "Текущие счета физических лиц", Nature = AccountNature.Passive },
            new ChartAccount { Id = 4, Code = ChartCodes.DemandDeposits, NameEn = "Demand deposits of individuals", NameRu = "Вклады до востребования физических лиц", Nature = AccountNature.Passive },
            new ChartAccount { Id = 5, Code = ChartCodes.TermDeposits, NameEn = "Term deposits of individuals", NameRu = "Срочные вклады физических лиц", Nature = AccountNature.Passive },
            new ChartAccount { Id = 6, Code = ChartCodes.DemandInterest, NameEn = "Accrued interest on demand deposits", NameRu = "Начисленные проценты по вкладам до востребования", Nature = AccountNature.Passive },
            new ChartAccount { Id = 7, Code = ChartCodes.TermInterest, NameEn = "Accrued interest on term deposits", NameRu = "Начисленные проценты по срочным вкладам", Nature = AccountNature.Passive },
            new ChartAccount { Id = 8, Code = ChartCodes.DevelopmentFund, NameEn = "Bank development fund", NameRu = "Фонд развития банка", Nature = AccountNature.Passive });
    }
}
