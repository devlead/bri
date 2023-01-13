using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryModuleOutput(
        [property: JsonPropertyName("type")]
        string Type,
        [property: JsonPropertyName("value")]
        string Value,
        [property: JsonPropertyName("metadata")]
        AcrRepositoryModuleMetadata Metadata
    );