using System.Net.Http.Json;
using Bank.Common.Localization;

namespace Bank.Credits.Http;

internal static class LocaleHttp
{
    public static async Task<HttpResponseMessage> GetAsync(
        HttpClient http,
        string uri,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.AcceptLanguage.ParseAdd(RequestLocale.Current);
        return await http.SendAsync(request, cancellationToken);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        HttpClient http,
        string uri,
        T body,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.AcceptLanguage.ParseAdd(RequestLocale.Current);
        return await http.SendAsync(request, cancellationToken);
    }
}
