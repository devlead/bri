using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace BRI.Services.Acr;

public record TokenService(
    IHttpClientFactory HttpClientFactory,
    AzureTokenService AzureTokenService,
    ILogger<TokenService> Logger
    )
{
    static Azure.Core.AccessToken? cachedAccessToken = default;
    static string? cachedAcrAccessToken = default;

    private async Task<string> GetAzureToken()
    {
        if (cachedAccessToken.HasValue && (cachedAccessToken.Value.ExpiresOn - DateTimeOffset.UtcNow).TotalMinutes > 1)
        {
            return cachedAccessToken.Value.Token;
        }

        Logger.LogInformation("Getting azure token...");
        try
        {
            var accessToken = await AzureTokenService();
            cachedAccessToken = accessToken;
            return accessToken.Token;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to fetch Azure Access Token");
            throw;
        }
    }

    private async Task<string> GetAcrRefreshToken(string containerService)
    {
        if (
            !string.IsNullOrWhiteSpace(cachedAcrAccessToken) &&
            cachedAccessToken.HasValue &&
            (cachedAccessToken.Value.ExpiresOn - DateTimeOffset.UtcNow).TotalMinutes > 1)
        {
            return cachedAcrAccessToken;
        }

        Logger.LogInformation("Converting azure token to acr token...");
        try
        {
            var token = await PostToGetAcrToken<RefreshToken>(
                $"https://{containerService}/oauth2/exchange",
                new KeyValuePair<string, string>[]
                    {
                    new ("service", containerService),
                    new ("grant_type", "access_token"),
                    new ("access_token", await GetAzureToken())
                    }
                );

            cachedAcrAccessToken = token;

            Logger.LogInformation("ACR token acquired");

            return token;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to exchange Azure Access Token");
            throw;
        }
    }

    private async Task<string> PostToGetAcrToken<T>(
        string url,
        KeyValuePair<string, string>[] nameValueCollection,
        [CallerMemberName]
        string name = nameof(PostToGetAcrToken)
        ) where T : IToken
    {
        using var httpClient = HttpClientFactory.CreateClient(name);

        var tokenResponse = await httpClient.PostAsync(
            url,
            new FormUrlEncodedContent(
               nameValueCollection
            )
            );

        tokenResponse.EnsureSuccessStatusCode();

        var result = await tokenResponse.Content.ReadFromJsonAsync<T>();

        ArgumentException.ThrowIfNullOrEmpty(result?.Token);

        return result.Token;
    }

    private async Task<string> GetAcrScopedToken(string containerService, string scope)
    {
        try
        {
            var token = await PostToGetAcrToken<AccessToken>(
                   $"https://{containerService}/oauth2/token",
                    new KeyValuePair<string, string>[]
                    {
                        new ("service", containerService),
                        new ("grant_type", "refresh_token"),
                        new ("scope", scope),
                        new ("refresh_token", await GetAcrRefreshToken(containerService))
                    }
                );

            return token;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get {Scope} Token", scope);
            throw;
        }
    }

    public async Task<string> GetCatalogToken(string containerService)
    {
        return await GetAcrScopedToken(containerService, "registry:catalog:*");
    }

    public async Task<string> GetRepoToken(string containerService, string repository)
    {
        return await GetAcrScopedToken(containerService, $"repository:{repository}:pull");
    }

    public async Task<T> CatalogHttpClientGetAsync<T>(
        string containerService,
        string url,
        string? accept = null
        )
    {
        using var catalogHttpClient = GetBearerTokenHttpClient(
           await GetCatalogToken(containerService),
           accept
           );

        return await GetFromJsonAsync<T>(catalogHttpClient, url);
    }

    public async Task<T> RepoHttpClientGetAsync<T>(
        string containerService,
        string repository,
        string url,
        string? accept = null
        )
    {
        using var repoHttpClient = GetBearerTokenHttpClient(
            await GetRepoToken(containerService, repository),
            accept
            );

        return await GetFromJsonAsync<T>(repoHttpClient, url);
    }

    private static async Task<T> GetFromJsonAsync<T>(HttpClient httpClient, string url)
    {
        var result = await httpClient.GetFromJsonAsync<T>(url);

        ArgumentNullException.ThrowIfNull(result);

        return result;
    }

    private HttpClient GetBearerTokenHttpClient(
        string bearerToken,
        string? accept,
        [CallerMemberName]
        string name = nameof(GetBearerTokenHttpClient)
        )
    {
        var bearerHttpClient = HttpClientFactory.CreateClient(name);

        bearerHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
           "Bearer",
           bearerToken
        );

        if (!string.IsNullOrWhiteSpace(accept))
        {
            bearerHttpClient.DefaultRequestHeaders.Accept.TryParseAdd(accept);
        }

        return bearerHttpClient;
    }
}
