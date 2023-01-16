using Azure.Core;
using Azure.Identity;
using BRI.Commands;
using BRI.Services;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Net.Http.Headers;

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
    .AddSingleton<BicepModuleMarkdownService>();


serviceCollection.AddHttpClient<AcrTokenService>();
serviceCollection.AddHttpClient<AcrCatalogService>();
serviceCollection.AddHttpClient<AcrRepositoryTagService>();
serviceCollection.AddHttpClient<AcrRepositoryManifestService>(
    client => client.DefaultRequestHeaders.Accept.TryParseAdd(
        "application/vnd.cncf.oras.artifact.manifest.v1+json;q=0.3, application/vnd.oci.image.manifest.v1+json;q=0.4, application/vnd.oci.artifact.manifest.v1+json;q=0.5, application/vnd.docker.distribution.manifest.v2+json;q=0.6, application/vnd.docker.distribution.manifest.list.v2+json;q=0.7"
        )
    );
serviceCollection.AddHttpClient<AcrRepositoryBlobService>();

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