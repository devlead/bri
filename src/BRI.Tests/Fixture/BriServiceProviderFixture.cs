﻿using Azure.Core;
using System.Text.Json;

namespace BRI.Tests.Fixture;

public static class BriServiceProviderFixture
{
    public static (T1, T2, T3, T4, T5) GetRequiredService<T1, T2, T3, T4, T5>(
       Func<IServiceCollection, IServiceCollection>? configure = null
       )    where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>(),
            provider.GetRequiredService<T4>(),
            provider.GetRequiredService<T5>()
            );
    }



    public static (T1, T2, T3, T4) GetRequiredService<T1, T2, T3, T4>(
       Func<IServiceCollection, IServiceCollection>? configure = null
       )    where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>(),
            provider.GetRequiredService<T4>()
            );
    }

    public static (T1, T2, T3) GetRequiredService<T1, T2, T3>(
        Func<IServiceCollection, IServiceCollection>? configure = null
        )   where T1 : notnull
            where T2 : notnull
            where T3 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>()
            );
    }

    public static (T1, T2) GetRequiredService<T1, T2>(
        Func<IServiceCollection, IServiceCollection>? configure = null
        )   where T1 : notnull
            where T2 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>()
            );
    }

    public static T GetRequiredService<T>(
        Func<IServiceCollection, IServiceCollection>? configure = null
        ) where T : notnull
        => GetServiceProvider(configure)
            .GetRequiredService<T>();

    public static ServiceProvider GetServiceProvider(Func<IServiceCollection, IServiceCollection>? configure)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddSingleton<AzureTokenService>(
                () => Task.FromResult(new AccessToken(nameof(AccessToken), DateTimeOffset.UtcNow.AddDays(1)))
            )
            .AddSingleton<TokenService>()
            .AddSingleton<CatalogService>()
            .AddSingleton<BlobService>()
            .AddSingleton<ManifestService>()
            .AddSingleton<TagService>()
            .AddMockHttpClient();

            return (configure?.Invoke(serviceCollection) ?? serviceCollection).BuildServiceProvider();
        }

    public static Module GetModule()
        => JsonSerializer.Deserialize<Module>(
            Constants.ACR.Response.Json.Repo.Module
            ) ?? throw new Exception($"Failed to get module.");

    public static Tag GetTag() => JsonSerializer.Deserialize<Tag>(
        Constants.ACR.Response.Json.Repo.Tag
        ) ?? throw new Exception($"Failed to get tag from json.");
}
