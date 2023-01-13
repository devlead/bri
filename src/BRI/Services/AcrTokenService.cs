using System.Net.Http.Json;
using BRI.Models;

namespace BRI.Services;

public record AcrTokenService(
    HttpClient HttpClient
    )
{
    public async Task<string> GetCatalogToken(string containerService)
    {
        var result = await HttpClient.GetFromJsonAsync<AcrAccessToken>($"https://{containerService}/oauth2/token?service={containerService}&scope=registry:catalog:*");

        ArgumentException.ThrowIfNullOrEmpty(result?.Token);

        return result.Token;
    }

    public async Task<string> GetRepoToken(string containerService, string repository)
    {
        var result = await HttpClient.GetFromJsonAsync<AcrAccessToken>($"https://{containerService}/oauth2/token?service={containerService}&scope=repository:{repository}:pull");

        ArgumentException.ThrowIfNullOrEmpty(result?.Token);

        return result.Token;
    }
}
