namespace BRI.Models.Acr.Repository.Module;

public record Parameter(
        [property: JsonPropertyName("type")]
        string Type,
        [property: JsonPropertyName("defaultValue")]
        JsonElement? DefaultValue,
        [property: JsonPropertyName("allowedValues")]
        string[] AllowedValues,
        [property: JsonPropertyName("metadata")]
        Metadata Metadata
    );
