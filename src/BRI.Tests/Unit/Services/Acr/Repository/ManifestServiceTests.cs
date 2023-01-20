namespace BRI.Tests.Unit.Services.Acr.Repository;

[TestFixture]
public class ManifestServiceTests
{
    [Test]
    public async Task GetManifestLayers()
    {
        // Given
        var (httpClient, tokenService) = BriServiceProviderFixture
                               .GetRequiredService<HttpClient, TokenService>();

        var manifestService = new ManifestService(
                                    TokenService: tokenService
                                ) with
                                {
                                    TokenService = tokenService
                                };

        // When
        var result = await manifestService.GetManifestLayers(
            Constants.ACR.ContainerService,
            Constants.ACR.Repo,
            Constants.ACR.Digest.Tag);

        // Then
        await Verify(result);
    }
}