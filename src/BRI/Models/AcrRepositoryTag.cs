using System;
using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryTag(
        [property: JsonPropertyName("name")]
        string Name,
        [property: JsonPropertyName("digest")]
        string Digest,
        [property: JsonPropertyName("createdTime")]
        DateTimeOffset CreatedTime,
        [property: JsonPropertyName("lastUpdateTime")]
        DateTimeOffset LastUpdateTime
    );
