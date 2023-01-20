namespace BRI.Services.Acr.Repository;

public record BlobService(TokenService TokenService)
{
    public async Task<Module> GetModule(string containerService, string repo, string digest)
    {
        return await TokenService
            .RepoHttpClientGetAsync<Module>(
                containerService,
                repo,
                $"https://{containerService}/v2/{repo}/blobs/{digest}"
            );
    }
}

