using Bank.Common.Accounting;
using Bank.Credits.Accounting;
using Bank.Credits.Commands.CloseBankingDay;
using Bank.Credits.Commands.CreateCreditContract;
using Bank.Credits.Data;
using Bank.Credits.Mapping;
using Bank.Credits.Queries.GetAccountsReport;
using Bank.Credits.Validation;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Credits.Support;

public sealed class SqliteTestDb : IDisposable
{
    public SqliteConnection Connection { get; }
    public CreditsDbContext Db { get; }
    public IMapper Mapper { get; } = new Mapper(CreditMapping.CreateConfig());
    public FakeClientsApi ClientsApi { get; } = FakeClientsApi.Seeded();
    public FakeDepositsApi DepositsApi { get; } = FakeDepositsApi.WithClientDepositAccounts();

    public SqliteTestDb()
    {
        Connection = new SqliteConnection($"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        Connection.Open();
        var options = new DbContextOptionsBuilder<CreditsDbContext>().UseSqlite(Connection).Options;
        Db = new CreditsDbContext(options);
        Db.Database.Migrate();
        CurrencyPositionSeeder.EnsureAsync(Db).GetAwaiter().GetResult();
    }

    public CreditWriteValidator CreditValidator()
        => new(Db, new CreditContractRequestValidator(), ClientsApi);

    public CreditLedger Ledger()
        => new(Db, new HardcodedExchangeRateProvider());

    public CreateCreditContractHandler CreateCreditContractHandler()
        => new(Db, CreditValidator(), Ledger(), new CreditCardIssuer(Db, Ledger()), ClientsApi);

    public CloseBankingDayHandler CloseBankingDayHandler()
        => new(Db, Ledger());

    public GetAccountsReportHandler GetAccountsReportHandler()
        => new(Db, DepositsApi);

    public void Dispose()
    {
        Db.Dispose();
        Connection.Dispose();
    }
}
