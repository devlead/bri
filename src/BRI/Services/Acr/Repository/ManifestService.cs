namespace BRI.Services.Acr.Repository;

public record ManifestService(TokenService TokenService)
{
    public async Task<ICollection<SchemaLayer>> GetManifestLayers(string containerService, string repo, string digest)
    {
        var result = await TokenService
           .RepoHttpClientGetAsync<Schema>(
               containerService,
               repo,
               $"https://{containerService}/v2/{repo}/manifests/{digest}",
               "application/vnd.cncf.oras.artifact.manifest.v1+json;q=0.3, application/vnd.oci.image.manifest.v1+json;q=0.4, application/vnd.oci.artifact.manifest.v1+json;q=0.5, application/vnd.docker.distribution.manifest.v2+json;q=0.6, application/vnd.docker.distribution.manifest.list.v2+json;q=0.7"
           );

        ArgumentNullException.ThrowIfNull(result?.Layers);

        return result
            .Layers
            .Where(layer => StringComparer.OrdinalIgnoreCase.Equals("application/vnd.ms.bicep.module.layer.v1+json", layer.MediaType))
            .ToArray();
    }
}
