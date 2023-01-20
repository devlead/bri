namespace BRI.Models.Acr.Token;

public record AccessToken(
        [property: JsonPropertyName("access_token")]
        string Token
    ) : IToken;
