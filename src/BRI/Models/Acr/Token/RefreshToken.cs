namespace BRI.Models.Acr.Token;

public record RefreshToken(
        [property: JsonPropertyName("refresh_token")]
        string Token
    ) : IToken;
