using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryTags(
        [property: JsonPropertyName("tags")]
        AcrRepositoryTag[] Tags
    );
