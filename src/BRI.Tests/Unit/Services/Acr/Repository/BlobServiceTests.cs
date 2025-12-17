namespace BRI.Tests.Unit.Services.Acr.Repository;

[TestFixture]
public class BlobServiceTests
{
    [Test]
    public async Task GetModule()
    {
        // Given
        var tokenService = BriServiceProviderFixture
                               .GetRequiredService<TokenService>();

        var blobService = new BlobService(
            TokenService: tokenService
        ) with
        {
            TokenService = tokenService
        };

        // When
        var result = await blobService.GetModule(
            Constants.ACR.ContainerService,
            Constants.ACR.Repo,
            Constants.ACR.Digest.Blob);

        // Then
        await Verify(result);
    }


    [Test]
    public async Task GetModuleOptionIfExists()
    {
        // Given
        var tokenService = BriServiceProviderFixture
                               .GetRequiredService<TokenService>();

        var blobService = new BlobService(
            TokenService: tokenService
        ) with
        {
            TokenService = tokenService
        };

        // When
        var result = await blobService.GetModule(
            Constants.ACR.ContainerService,
            Constants.ACR.Repo,
            Constants.ACR.Digest.BlobOptionIfExists);

        // Then
        await Verify(result);
    }
}
