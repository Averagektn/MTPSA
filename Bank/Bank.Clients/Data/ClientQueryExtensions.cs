using Bank.Clients.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Clients.Data;

public static class ClientQueryExtensions
{
    public static IQueryable<Client> WithLookups(this IQueryable<Client> clients) =>
        clients
            .Include(c => c.ResidenceCity)
            .Include(c => c.RegistrationCity)
            .Include(c => c.MaritalStatus)
            .Include(c => c.Citizenship)
            .Include(c => c.Disability);
}
