namespace BRI.Models.Acr.Repository;

public record Repositores(
        [property: JsonPropertyName("repositories")]
        string[] Repositories
    );
