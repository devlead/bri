using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryModuleMetadata(
        [property: JsonPropertyName("description")]
        string Description
    );
