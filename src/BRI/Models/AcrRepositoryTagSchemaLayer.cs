using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryTagSchemaLayer(
        [property: JsonPropertyName("mediaType")]
        string MediaType,
        [property: JsonPropertyName("digest")]
        string Digest,
        [property: JsonPropertyName("size")]
        long Size
    );
