namespace BRI.Models.Acr.Repository.Tag;

public record SchemaLayer(
        [property: JsonPropertyName("mediaType")]
        string MediaType,
        [property: JsonPropertyName("digest")]
        string Digest,
        [property: JsonPropertyName("size")]
        long Size
    );
