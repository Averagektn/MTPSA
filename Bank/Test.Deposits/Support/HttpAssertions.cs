using System.Net.Http.Json;

namespace Tests.Deposits.Support;

public static class HttpAssertions
{
    public static async Task<ValidationProblemDto> ReadProblem(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDto>(DepositSamples.JsonOptions);
        problem.Should().NotBeNull();
        return problem!;
    }
}
