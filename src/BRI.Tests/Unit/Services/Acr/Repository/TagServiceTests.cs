namespace BRI.Tests.Unit.Services.Acr.Repository;

[TestFixture]
public class TagServiceTests
{
    [Test]
    public async Task GetManifestLayers()
    {
        // Given
        var (httpClient, tokenService) = BriServiceProviderFixture
                               .GetRequiredService<HttpClient, TokenService>();

        var tagService = new TagService(
                                    TokenService: tokenService
                                ) with
                                {
                                    TokenService = tokenService
                                };

        // When
        var result = await tagService.GetTags(
            Constants.ACR.ContainerService,
            Constants.ACR.Repo);

        // Then
        await Verify(result);
    }
}
