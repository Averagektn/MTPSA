using Bank.Clients.Commands.CreateClient;
using Bank.Clients.Commands.DeleteClient;
using Bank.Clients.Commands.UpdateClient;
using Bank.Clients.Data;
using Bank.Clients.Mapping;
using Bank.Clients.Queries.GetClient;
using Bank.Clients.Queries.GetClients;
using Bank.Clients.Queries.GetDictionaries;
using Bank.Clients.Validation;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Clients.Support;

public sealed class SqliteTestDb : IDisposable
{
    public SqliteConnection Connection { get; }
    public BankDbContext Db { get; }
    public IMapper Mapper { get; } = new Mapper(ClientMapping.CreateConfig());
    public FakeDepositsApi DepositsApi { get; } = new();
    public FakeCreditsApi CreditsApi { get; } = new();

    public SqliteTestDb()
    {
        Connection = new SqliteConnection($"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        Connection.Open();
        var options = new DbContextOptionsBuilder<BankDbContext>().UseSqlite(Connection).Options;
        Db = new BankDbContext(options);
        Db.Database.Migrate();
    }

    public ClientWriteValidator WriteValidator()
        => new(Db, new ClientRequestValidator());

    public CreateClientHandler CreateClientHandler()
        => new(Db, WriteValidator(), Mapper);

    public UpdateClientHandler UpdateClientHandler()
        => new(Db, WriteValidator(), Mapper);

    public DeleteClientHandler DeleteClientHandler()
        => new(Db, DepositsApi, CreditsApi);

    public GetClientHandler GetClientHandler()
        => new(Db, Mapper);

    public GetClientsHandler GetClientsHandler()
        => new(Db, Mapper);

    public GetDictionariesHandler GetDictionariesHandler()
        => new(Db, Mapper);

    public void Dispose()
    {
        Db.Dispose();
        Connection.Dispose();
    }
}
