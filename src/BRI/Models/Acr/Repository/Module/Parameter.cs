using Newtonsoft.Json.Linq;

namespace BRI.Models.Acr.Repository.Module;

public record Parameter(
        [property: JsonPropertyName("type")]
        string Type,
        [property: JsonPropertyName("defaultValue")]
        JsonElement? DefaultValue,
        [property: JsonPropertyName("allowedValues")]
        string[] AllowedValues,
        [property: JsonPropertyName("metadata")]
        Metadata Metadata,
        [property: JsonPropertyName("maxValue")]
        int? MaxValue,
        [property: JsonPropertyName("minValue")]
        int? MinValue,
        [property: JsonPropertyName("maxLength")]
        int? MaxLength,
        [property: JsonPropertyName("minLength")]
int? MinLength
    )
{
    public ParsedArmType ParsedType { get; } = Type switch
    {
        "secureString" => new(true, "string"),
        "secureObject" => new(true, "object"),
        _ => new(false, Type)
    };
}
