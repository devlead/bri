using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRefreshToken(
        [property: JsonPropertyName("refresh_token")]
        string Token
    );
