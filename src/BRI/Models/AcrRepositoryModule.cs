using System.Text.Json;
using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositoryModule(
        [property: JsonPropertyName("parameters")]
        Dictionary<string, AcrRepositoryModuleParameter> Parameters,
        [property: JsonPropertyName("outputs")]
        Dictionary<string, AcrRepositoryModuleOutput> Outputs,
        [property: JsonPropertyName("resources")]
        Dictionary<string, JsonElement>[] Resources
     );
