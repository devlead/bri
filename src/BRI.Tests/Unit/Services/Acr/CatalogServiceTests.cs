using NSubstitute;

namespace BRI.Tests.Unit.Services.Acr;

[TestFixture]
public class CatalogServiceTests
{
    [Test]
    public async Task GetRepositories()
    {
        // Given
        var tokenService = BriServiceProviderFixture
                                .GetRequiredService<TokenService>();

        var catalogService = new CatalogService(
                                    TokenService: tokenService
                                ) with
                                {
                                    TokenService = tokenService
                                };

        // When
        var result = await catalogService.GetRepositories(Constants.ACR.ContainerService);

        // Then
        await Verify(result);
    }
}
