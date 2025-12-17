using System.Text.Json.Nodes;

namespace BRI.Models.Acr.Repository.Module;

public record Module(
        [property: JsonPropertyName("languageVersion")]
        string? LanguageVersion,
        [property: JsonPropertyName("parameters")]
        Dictionary<string, Parameter> Parameters,
        [property: JsonPropertyName("outputs")]
        Dictionary<string, Output> Outputs,
        [property: JsonPropertyName("resources")]
        JsonNode Resources,
        [property: JsonPropertyName("metadata")]
        Template.Metadata.Metadata Metadata
     );
