using System.Text.Json;
using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryModuleParameter(
        [property: JsonPropertyName("type")]
        string Type,
        [property: JsonPropertyName("defaultValue")]
        JsonElement? DefaultValue,
        [property: JsonPropertyName("allowedValues")]
        string[] AllowedValues,
        [property: JsonPropertyName("metadata")]
        AcrRepositoryModuleMetadata Metadata
    );
