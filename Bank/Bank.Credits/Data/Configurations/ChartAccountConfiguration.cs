using Bank.Credits.Accounting;
using Bank.Credits.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Credits.Data.Configurations;

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
            new ChartAccount { Id = 3, Code = ChartCodes.ShortTermLoans, NameEn = "Short-term loans to individuals", NameRu = "Краткосрочные кредиты физическим лицам", Nature = AccountNature.Active },
            new ChartAccount { Id = 4, Code = ChartCodes.LongTermLoans, NameEn = "Long-term loans to individuals", NameRu = "Долгосрочные кредиты физическим лицам", Nature = AccountNature.Active },
            new ChartAccount { Id = 5, Code = ChartCodes.ShortTermInterest, NameEn = "Accrued interest on short-term loans to individuals", NameRu = "Начисленные проценты по краткосрочным кредитам физическим лицам", Nature = AccountNature.Active },
            new ChartAccount { Id = 6, Code = ChartCodes.LongTermInterest, NameEn = "Accrued interest on long-term loans to individuals", NameRu = "Начисленные проценты по долгосрочным кредитам физическим лицам", Nature = AccountNature.Active },
            new ChartAccount { Id = 7, Code = ChartCodes.DevelopmentFund, NameEn = "Bank development fund", NameRu = "Фонд развития банка", Nature = AccountNature.Passive },
            new ChartAccount { Id = 8, Code = ChartCodes.CardAccount, NameEn = "Card accounts of individuals", NameRu = "Карт-счета физических лиц", Nature = AccountNature.Passive },
            new ChartAccount { Id = 9, Code = ChartCodes.MobileSettlements, NameEn = "Settlements with mobile operators", NameRu = "Расчёты с операторами связи", Nature = AccountNature.Passive });
    }
}
