using System.Net.Http.Json;
using BRI.Models;

namespace BRI.Services;

public record AcrRepositoryManifestService(
    AcrTokenService AcrTokenService,
    HttpClient HttpClient
    )
{
    public async Task<ICollection<AcrRepositoryTagSchemaLayer>> GetManifestLayers(string containerService, string repo, string digest)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            await AcrTokenService.GetRepoToken(containerService, repo)
            );

        var result = await HttpClient.GetFromJsonAsync<AcrRepositoryTagSchema>($"https://{containerService}/v2/{repo}/manifests/{digest}");

        ArgumentNullException.ThrowIfNull(result?.Layers);

        return result
            .Layers
            .Where(layer => StringComparer.OrdinalIgnoreCase.Equals("application/vnd.ms.bicep.module.layer.v1+json", layer.MediaType))
            .ToArray();
    }
}
