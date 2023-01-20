namespace BRI.Services.Acr.Repository;

public record RepositoryService(
    BlobService Blob,
    ManifestService Manifest,
    TagService Tag
    );