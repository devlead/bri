using BRI.Commands;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration;
using Spectre.Console.Cli.Extensions.DependencyInjection;
using Azure.Core;
using Azure.Identity;

var serviceCollection = new ServiceCollection()
    .AddCakeCore()
    .AddLogging(configure =>
            configure
                .AddSimpleConsole(opts =>
                {
                    opts.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                })
                .AddConfiguration(
                new ConfigurationBuilder()
                    .Add(new MemoryConfigurationSource
                    {
                        InitialData = new Dictionary<string, string?>
                        {
                            { "LogLevel:System.Net.Http.HttpClient", "Warning" }
                        }
                    })
                    .Build()
            ))
    .AddSingleton<AzureTokenService>(
        async () =>
        {
            var tokenCredential = new DefaultAzureCredential();
            var accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(scopes: new string[] { "https://management.azure.com/.default" })
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

serviceCollection.AddHttpClient();

using var registrar = new DependencyInjectionRegistrar(serviceCollection);
var app = new CommandApp(registrar);

app.Configure(
    config =>
    {
        config.ValidateExamples();

        config.AddCommand<InventoryCommand>("inventory")
                .WithDescription("Example inventory command.")
                .WithExample(new[] { "inventory", "test.azurecr.io", "outputpath" });
    });

return await app.RunAsync(args);