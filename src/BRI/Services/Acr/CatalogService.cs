namespace BRI.Services.Acr;

public record CatalogService(TokenService TokenService)
{
    public async Task<ICollection<string>> GetRepositories(string containerService)
    {
        var result = await TokenService
            .CatalogHttpClientGetAsync<Repositores>(
                containerService,
                $"https://{containerService}/acr/v1/_catalog"
            );

        ArgumentNullException.ThrowIfNull(result?.Repositories);

        return result.Repositories;
    }
}
