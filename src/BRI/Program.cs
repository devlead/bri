using BRI.Commands;
using Azure.Core;
using Azure.Identity;

public partial class Program
{
    static partial void AddServices(IServiceCollection services)
    {
        services
            .AddCakeCore()
            .AddSingleton<AzureTokenService>(
                async () =>
                {
                    var tokenCredential = new DefaultAzureCredential();
                    var accessToken = await tokenCredential.GetTokenAsync(
                        new TokenRequestContext(scopes: ["https://management.azure.com/.default"])
                    );
                    return accessToken;
                }
            )
            .AddSingleton<BicepModuleMarkdownService>()
            .AddSingleton<RepositoryService>()
            .AddSingleton<CatalogService>()
            .AddSingleton<TagService>()
            .AddSingleton<ManifestService>()
            .AddSingleton<BlobService>()
            .AddSingleton<InventoryCommand>()
            .AddSingleton<TokenService>();

        services.AddHttpClient();
    }

    static partial void ConfigureApp(AppServiceConfig appServiceConfig)
    {
        appServiceConfig.AddCommand<InventoryCommand>("inventory")
                .WithDescription("Example inventory command.")
                .WithExample(["inventory", "test.azurecr.io", "outputpath"]);
    }
}