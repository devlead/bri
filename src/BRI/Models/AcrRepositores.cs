using System.Text.Json.Serialization;

namespace BRI.Models;

public record AcrRepositores(
        [property: JsonPropertyName("repositories")]
        string[] Repositories
    );
