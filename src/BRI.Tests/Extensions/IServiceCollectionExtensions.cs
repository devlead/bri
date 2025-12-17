using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;
using VerifyTests.Http;

namespace BRI.Tests.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCakeFakes(
        this IServiceCollection services,
        Action<FakeFileSystem>? configureFileSystem = null
        )
    {
        var configuration = new FakeConfiguration();
        
        var environment = FakeEnvironment.CreateUnixEnvironment();
        
        var fileSystem = new FakeFileSystem(environment);
        configureFileSystem?.Invoke(fileSystem);

        var globber = new Globber(fileSystem, environment);
        
        var log = new FakeLog();

        var Context = Substitute.For<ICakeContext>();
        Context.Configuration.Returns(configuration);
        Context.Environment.Returns(environment);
        Context.FileSystem.Returns(fileSystem);
        Context.Globber.Returns(globber);
        Context.Log.Returns(log);

        return services.AddSingleton<ICakeConfiguration>(configuration)
                                .AddSingleton<ICakeEnvironment>(environment)
                                .AddSingleton<FakeFileSystem>(fileSystem)
                                .AddSingleton<IFileSystem>(fileSystem)
                                .AddSingleton<IGlobber>(globber)
                                .AddSingleton<ICakeLog>(log)
                                .AddSingleton<ICakeRuntime>(environment.Runtime)
                                .AddSingleton<ICakeContext>(Context);
    }

    public static IServiceCollection AddMockHttpClient(this IServiceCollection services)
    {
        static HttpResponseMessage GetMockOauth2ExchangeResponse()
        => new()
        {
            Content = new StringContent(
                                Constants.ACR.Response.Json.Oauth2Exchange,
                                Encoding.UTF8,
                                Constants.MediaType.Json
                            )
        };
        static HttpResponseMessage GetMockOauth2TokenResponse()
            => new()
            {
                Content = new StringContent(
                                    Constants.ACR.Response.Json.Oauth2Token,
                                    Encoding.UTF8,
                                    Constants.MediaType.Json
                                )
            };
        static HttpResponseMessage GetMockCatalogResponse()
            => new()
            {
                Content = new StringContent(
                                    Constants.ACR.Response.Json.Catalog,
                                    Encoding.UTF8,
                                    Constants.MediaType.Json
                                )
            };

        static HttpResponseMessage GetMockRepoTagsResponse()
            => new()
            {
                Content = new StringContent(
                                    Constants.ACR.Response.Json.Repo.Tags,
                                    Encoding.UTF8,
                                    Constants.MediaType.Json
                                )
            };

        static HttpResponseMessage GetMockRepoManifestsResponse()
            => new()
            {
                Content = new StringContent(
                                    Constants.ACR.Response.Json.Manifests,
                                    Encoding.UTF8,
                                    Constants.MediaType.Json
                                )
            };

        static HttpResponseMessage GetMockRepoBlobResponse(bool optionIfExists)
        => new()
        {
            Content = (optionIfExists)
            ? new StringContent(
                                    Constants.ACR.Response.Json.Repo.ModuleOptionIfExists,
                                    Encoding.UTF8,
                                    Constants.MediaType.BicepModuleLayer
                                )
            : new StringContent(
                                    Constants.ACR.Response.Json.Repo.Module,
                                    Encoding.UTF8,
                                    Constants.MediaType.BicepModuleLayer
                                )
        };

        static MockHttpClient CreateClient()
            => new MockHttpClient(
                    request => request switch
                    {
                        {
                            Method.Method: Constants.Request.Method.Post,
                            RequestUri.AbsoluteUri: Constants.ACR.Request.Uri.Oauth2.Exchange
                        } => GetMockOauth2ExchangeResponse(),

                        {
                            Method.Method: Constants.Request.Method.Post,
                            RequestUri.AbsoluteUri: Constants.ACR.Request.Uri.Oauth2.Token
                        } => GetMockOauth2TokenResponse(),

                        {
                            Method.Method: Constants.Request.Method.Get,
                            RequestUri.AbsoluteUri: Constants.ACR.Request.Uri.Catalog
                        } => GetMockCatalogResponse(),

                        {
                            Method.Method: Constants.Request.Method.Get,
                            RequestUri.AbsoluteUri: Constants.ACR.Request.Uri.Tag
                        } => GetMockRepoTagsResponse(),

                        {
                            Method.Method: Constants.Request.Method.Get,
                            RequestUri.AbsoluteUri: Constants.ACR.Request.Uri.Manifest
                        } => GetMockRepoManifestsResponse(),

                        {
                            Method.Method: Constants.Request.Method.Get,
                            RequestUri.AbsoluteUri: Constants.ACR.Request.Uri.Blob
                        } => GetMockRepoBlobResponse(false),

                        {
                            Method.Method: Constants.Request.Method.Get,
                            RequestUri.AbsoluteUri: Constants.ACR.Request.Uri.BlobOptionIfExists
                        } => GetMockRepoBlobResponse(true),

                        _ => new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.NotFound
                        }
                    }
                );

        return services
            .AddSingleton<HttpClient>(
            _ => CreateClient()
            )
            .AddSingleton<IHttpClientFactory>(
             _ =>
             {
                 var httpClientFactory = Substitute.For<IHttpClientFactory>();
                 httpClientFactory
                    .CreateClient(null!)
                    .ReturnsForAnyArgs(_ => CreateClient());

                 httpClientFactory
                    .CreateClient()
                    .Returns(_ => CreateClient());

                 return httpClientFactory;
             }
            );;
    }
}
