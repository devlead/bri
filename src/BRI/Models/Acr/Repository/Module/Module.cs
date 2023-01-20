namespace BRI.Models.Acr.Repository.Module;

public record Module(
        [property: JsonPropertyName("parameters")]
        Dictionary<string, Parameter> Parameters,
        [property: JsonPropertyName("outputs")]
        Dictionary<string, Output> Outputs,
        [property: JsonPropertyName("resources")]
        Dictionary<string, JsonElement>[] Resources,
        [property: JsonPropertyName("metadata")]
        Template.Metadata.Metadata Metadata
     );
