using Bank.Clients.Models;
using Bank.Common.Dtos.Request;
using Bank.Common.Dtos.Response;
using Bank.Common.Localization;
using Mapster;

namespace Bank.Clients.Mapping;

public sealed class ClientMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Client, ClientListItem>()
            .Map(
                item => item.ResidenceCity,
                client => RequestLocale.Pick(client.ResidenceCity.NameEn, client.ResidenceCity.NameRu));

        config.NewConfig<Client, ClientResponse>()
            .Map(
                response => response.ResidenceCity,
                client => RequestLocale.Pick(client.ResidenceCity.NameEn, client.ResidenceCity.NameRu))
            .Map(
                response => response.RegistrationCity,
                client => RequestLocale.Pick(client.RegistrationCity.NameEn, client.RegistrationCity.NameRu))
            .Map(
                response => response.MaritalStatus,
                client => RequestLocale.Pick(client.MaritalStatus.NameEn, client.MaritalStatus.NameRu))
            .Map(
                response => response.Citizenship,
                client => RequestLocale.Pick(client.Citizenship.NameEn, client.Citizenship.NameRu))
            .Map(
                response => response.Disability,
                client => RequestLocale.Pick(client.Disability.NameEn, client.Disability.NameRu));

        config.NewConfig<ClientRequest, Client>()
            .Ignore(client => client.Id)
            .Ignore(client => client.ResidenceCity)
            .Ignore(client => client.RegistrationCity)
            .Ignore(client => client.MaritalStatus)
            .Ignore(client => client.Citizenship)
            .Ignore(client => client.Disability)
            .Map(client => client.BirthDate, request => request.ParsedBirthDate!.Value)
            .Map(client => client.IssueDate, request => request.ParsedIssueDate!.Value);

        config.NewConfig<City, LookupItem>()
            .Map(item => item.Name, city => RequestLocale.Pick(city.NameEn, city.NameRu));
        config.NewConfig<MaritalStatus, LookupItem>()
            .Map(item => item.Name, status => RequestLocale.Pick(status.NameEn, status.NameRu));
        config.NewConfig<Citizenship, LookupItem>()
            .Map(item => item.Name, citizenship => RequestLocale.Pick(citizenship.NameEn, citizenship.NameRu));
        config.NewConfig<Disability, LookupItem>()
            .Map(item => item.Name, disability => RequestLocale.Pick(disability.NameEn, disability.NameRu));
    }

    public static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(ClientMapping).Assembly);
        return config;
    }
}
