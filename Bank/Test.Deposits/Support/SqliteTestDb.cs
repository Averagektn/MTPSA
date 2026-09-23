using Bank.Common.Accounting;
using Bank.Deposits.Accounting;
using Bank.Deposits.Commands.CloseBankingDay;
using Bank.Deposits.Commands.CreateDepositContract;
using Bank.Deposits.Data;
using Bank.Deposits.Mapping;
using Bank.Deposits.Queries.GetAccountsReport;
using Bank.Deposits.Validation;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Deposits.Support;

public sealed class SqliteTestDb : IDisposable
{
    public SqliteConnection Connection { get; }
    public DepositsDbContext Db { get; }
    public IMapper Mapper { get; } = new Mapper(DepositMapping.CreateConfig());
    public FakeClientsApi ClientsApi { get; } = FakeClientsApi.Seeded();

    public SqliteTestDb()
    {
        Connection = new SqliteConnection($"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        Connection.Open();
        var options = new DbContextOptionsBuilder<DepositsDbContext>().UseSqlite(Connection).Options;
        Db = new DepositsDbContext(options);
        Db.Database.Migrate();
        CurrencyPositionSeeder.EnsureAsync(Db).GetAwaiter().GetResult();
    }

    public DepositWriteValidator DepositValidator()
        => new(Db, new DepositContractRequestValidator(), ClientsApi);

    public DepositLedger Ledger()
        => new(Db, new HardcodedExchangeRateProvider());

    public CreateDepositContractHandler CreateDepositContractHandler()
        => new(Db, DepositValidator(), Ledger(), ClientsApi);

    public CloseBankingDayHandler CloseBankingDayHandler()
        => new(Db, Ledger());

    public GetAccountsReportHandler GetAccountsReportHandler()
        => new(Db);

    public void Dispose()
    {
        Db.Dispose();
        Connection.Dispose();
    }
}
