namespace BRI.Models.Acr.Repository.Module.Template.Metadata;

public record Documentation(
    [property: JsonPropertyName("summary")]
    string Summary,
    [property: JsonPropertyName("description")]
    string Description
    );