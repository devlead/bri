using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrAccessToken(
        [property: JsonPropertyName("access_token")]
        string Token
    );
