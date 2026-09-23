using System.Net;
using System.Net.Http.Json;
using Bank.Common.Localization;
using Bank.Credits.Http;

namespace Tests.Credits.Unit.Http;

[TestClass]
public sealed class HttpDepositsApiTests
{
    [TestMethod]
    public async Task Forwards_current_locale_as_accept_language()
    {
        RequestLocale.SetFromAcceptLanguage("ru");
        var handler = new CaptureHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://deposits.test/") };
        var api = new HttpDepositsApi(http);

        var result = await api.GetAccountsAsync();

        result.IsSuccess.Should().BeTrue();
        handler.AcceptLanguage.Should().Be("ru");
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public string? AcceptLanguage { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AcceptLanguage = request.Headers.AcceptLanguage.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(Array.Empty<object>())
            });
        }
    }
}
