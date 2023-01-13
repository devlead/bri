using System.Net.Http.Json;
using BRI.Models;

namespace BRI.Services;

public record AcrRepositoryTagService(
    AcrTokenService AcrTokenService,
    HttpClient HttpClient
    )
{
    public async Task<ICollection<AcrRepositoryTag>> GetTags(string containerService, string repo, int tagLimitNumber = 1)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            await AcrTokenService.GetRepoToken(containerService, repo)
            );

        var result = await HttpClient.GetFromJsonAsync<AcrRepositoryTags>(FormattableString.Invariant($"https://{containerService}/acr/v1/{repo}/_tags?orderby=timedesc&n={tagLimitNumber}"));

        ArgumentNullException.ThrowIfNull(result?.Tags);

        return result.Tags;
    }
}
