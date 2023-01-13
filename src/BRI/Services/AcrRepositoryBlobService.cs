using System.Net.Http.Json;
using BRI.Models;

namespace BRI.Services;

public record AcrRepositoryBlobService(
    AcrTokenService AcrTokenService,
    HttpClient HttpClient
    )
{
    public async Task<AcrRepositoryModule> GetModule(string containerService, string repo, string digest)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
           "Bearer",
           await AcrTokenService.GetRepoToken(containerService, repo)
           );

        var result = await HttpClient.GetFromJsonAsync<AcrRepositoryModule>($"https://{containerService}/v2/{repo}/blobs/{digest}");

        ArgumentNullException.ThrowIfNull(result);

        return result;
    }
}
