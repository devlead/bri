namespace BRI.Models.Acr.Repository.Module.Template.Metadata;

public record Metadata(
    [property: JsonPropertyName("_generator")]
    Generator? Generator,
    [property: JsonPropertyName("documentation")]
    Documentation? Documentation
    );
