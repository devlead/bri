using System.Net.Http.Json;
using Azure.Core;
using Azure.Identity;
using BRI.Models;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

namespace BRI.Services;

public record AcrTokenService(
    HttpClient HttpClient,
    ILogger<AcrTokenService> Logger
    )
{
    static AccessToken? cachedAccessToken = default;
    static string? cachedAcrAccessToken = default;

    private static async Task<string> GetAzureToken(ILogger logger)
    {
        if (cachedAccessToken.HasValue && (cachedAccessToken.Value.ExpiresOn - DateTimeOffset.UtcNow).TotalMinutes > 1)
        {
            return cachedAccessToken.Value.Token;
        }

        logger.LogInformation("Getting azure token...");
        try
        {
            var tokenCredential = new DefaultAzureCredential();
            var accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(scopes: new string[] { "https://management.azure.com/.default" })
            );
            cachedAccessToken = accessToken;

            logger.LogInformation("Azure token acquired, expires on {ExpiresOn}.", accessToken.ExpiresOn.ToLocalTime());

            return accessToken.Token;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch Azure Access Token");
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
            var refreshTokenResponse = await HttpClient.PostAsync(
                $"https://{containerService}/oauth2/exchange",
                new FormUrlEncodedContent(
                    new KeyValuePair<string, string>[]
                    {
                    new ("service", containerService),
                    new ("grant_type", "access_token"),
                    new ("access_token", await GetAzureToken(Logger))
                    }
                )
                );

            refreshTokenResponse.EnsureSuccessStatusCode();

            var result = await refreshTokenResponse.Content.ReadFromJsonAsync<AcrRefreshToken>();

            ArgumentException.ThrowIfNullOrEmpty(result?.Token);

            cachedAcrAccessToken = result.Token;

            Logger.LogInformation("ACR token acquired");

            return result.Token;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to exchange Azure Access Token");
            throw;
        }
    }

    private async Task<string> GetAcrScopedToken(string containerService, string scope)
    {
        var accessTokenResponse = await HttpClient.PostAsync(
           $"https://{containerService}/oauth2/token",
           new FormUrlEncodedContent(
               new KeyValuePair<string, string>[]
               {
                    new ("service", containerService),
                    new ("grant_type", "refresh_token"),
                    new ("scope", scope),
                    new ("refresh_token", await GetAcrRefreshToken(containerService))
               }
           )
           );

        try
        {
            accessTokenResponse.EnsureSuccessStatusCode();

            var result = await accessTokenResponse.Content.ReadFromJsonAsync<AcrAccessToken>();

            ArgumentException.ThrowIfNullOrEmpty(result?.Token);

            return result.Token;
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
}
