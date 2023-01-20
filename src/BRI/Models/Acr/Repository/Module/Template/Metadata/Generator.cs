namespace BRI.Models.Acr.Repository.Module.Template.Metadata;

public record Generator(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("templateHash")]
    string TemplateHash
    );
