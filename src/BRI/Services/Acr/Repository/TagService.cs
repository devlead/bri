namespace BRI.Services.Acr.Repository;

public record TagService(TokenService TokenService)
{
    public async Task<ICollection<Tag>> GetTags(string containerService, string repo, int tagLimitNumber = 1)
    {
        var result = await TokenService
           .RepoHttpClientGetAsync<RepositoryTags>(
               containerService,
               repo,
               FormattableString.Invariant($"https://{containerService}/acr/v1/{repo}/_tags?orderby=timedesc&n={tagLimitNumber}")
           );

        ArgumentNullException.ThrowIfNull(result?.Tags);

        return result.Tags;
    }
}
