using Bank.Common.Dtos.Response;
using Bank.Common.Errors;
using Bank.Credits.Http;
using FluentResults;

namespace Tests.Credits.Support;

public sealed class FakeClientsApi : IClientsApi
{
    public static FakeClientsApi Seeded()
    {
        var api = new FakeClientsApi();
        for (var id = 1; id <= 6; id++)
        {
            api.Add(new ClientListItem(
                id,
                $"Client{id}",
                "Test",
                "X",
                new DateOnly(2000, 1, 1),
                "AB",
                "1234567",
                $"ID{id}",
                "Minsk",
                null,
                null));
        }

        return api;
    }

    private readonly Dictionary<int, ClientListItem> _clients = [];

    public void Add(ClientListItem client)
        => _clients[client.Id] = client;

    public Task<Result<ClientResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_clients.TryGetValue(id, out var client))
        {
            return Task.FromResult(Result.Fail<ClientResponse>(new ValidationError("clientId", "clientNotFound")));
        }

        return Task.FromResult(Result.Ok(ToResponse(client)));
    }

    public Task<Result<IReadOnlyList<ClientListItem>>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Ok<IReadOnlyList<ClientListItem>>(_clients.Values.OrderBy(c => c.Id).ToList()));

    private static ClientResponse ToResponse(ClientListItem client)
        => new(
            client.Id,
            client.LastName,
            client.FirstName,
            client.Patronymic,
            client.BirthDate,
            client.PassportSeries,
            client.PassportNumber,
            "",
            default,
            client.IdentificationNumber,
            "",
            1,
            client.ResidenceCity,
            "",
            1,
            "",
            null,
            client.MobilePhone,
            client.Email,
            null,
            null,
            1,
            "",
            1,
            "",
            1,
            "",
            false,
            null);
}
