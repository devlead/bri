namespace BRI.Models.Acr.Repository.Tag;

public record Schema(
        [property: JsonPropertyName("layers")]
        SchemaLayer[] Layers
    );
