namespace BRI.Tests.Unit.Services.Acr;

[TestFixture]
public class TokenServiceTests
{
    private TokenService TokenService { get; set; } = null!;

    [SetUp]
    public void Init()
    {
        var (
            httpClientFactory,
            azureTokenService,
            logger
            ) = BriServiceProviderFixture
                    .GetRequiredService<IHttpClientFactory, AzureTokenService, ILogger<TokenService>>();

        TokenService = new TokenService(
                HttpClientFactory: httpClientFactory,
                AzureTokenService: azureTokenService,
                Logger: logger
            ) with
            {
                HttpClientFactory = httpClientFactory,
                AzureTokenService = azureTokenService,
                Logger = logger
            };
    }

    [Test]
    public async Task GetCatalogToken()
    {
        // Given / When
        var result = await TokenService.GetCatalogToken(Constants.ACR.ContainerService);

        // Then
        await Verify(result);
    }

    [Test]
    public async Task GetRepoToken()
    {
        // Given / When
        var result = await TokenService.GetRepoToken(Constants.ACR.ContainerService, Constants.ACR.Repo);

        // Then
        await Verify(result);
    }
}