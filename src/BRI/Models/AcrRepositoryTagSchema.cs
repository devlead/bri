using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryTagSchema(
        [property: JsonPropertyName("layers")]
        AcrRepositoryTagSchemaLayer[] Layers
    );
