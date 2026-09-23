using System.Net.Http.Json;

namespace Tests.Clients.Support;

public static class HttpAssertions
{
    public static async Task<ValidationProblemDto> ReadProblem(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDto>(ClientSamples.JsonOptions);
        problem.Should().NotBeNull();
        return problem!;
    }

    public static string ToPascal(string field)
        => char.ToUpperInvariant(field[0]) + field[1..];
}
