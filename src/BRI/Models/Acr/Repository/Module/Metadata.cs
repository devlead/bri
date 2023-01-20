namespace BRI.Models.Acr.Repository.Module;

public record Metadata(
        [property: JsonPropertyName("description")]
        string Description
    );
