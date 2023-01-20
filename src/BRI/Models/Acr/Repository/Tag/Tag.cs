namespace BRI.Models.Acr.Repository.Tag;

public record Tag(
        [property: JsonPropertyName("name")]
        string Name,
        [property: JsonPropertyName("digest")]
        string Digest,
        [property: JsonPropertyName("createdTime")]
        DateTimeOffset CreatedTime,
        [property: JsonPropertyName("lastUpdateTime")]
        DateTimeOffset LastUpdateTime
    );
