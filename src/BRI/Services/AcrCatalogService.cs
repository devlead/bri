using System.Net.Http.Json;
using BRI.Models;

namespace BRI.Services;

public record AcrCatalogService(
    AcrTokenService AcrTokenService,
    HttpClient HttpClient
    )
{
    public async Task<ICollection<string>> GetRepositories(string containerService)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            await AcrTokenService.GetCatalogToken(containerService)
            );

        var result = await HttpClient.GetFromJsonAsync<AcrRepositores>($"https://{containerService}/acr/v1/_catalog");

        ArgumentNullException.ThrowIfNull(result?.Repositories);

        return result.Repositories;
    }
}
