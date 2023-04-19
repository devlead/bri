namespace BRI.Models.Acr.Repository.Module;

public record Output(
        [property: JsonPropertyName("type")]
        string Type,
        [property: JsonPropertyName("value")]
        JsonElement Value,
        [property: JsonPropertyName("metadata")]
        Metadata Metadata
    );