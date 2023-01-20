namespace BRI.Models.Acr.Repository.Tag;

public record RepositoryTags(
        [property: JsonPropertyName("tags")]
        Tag[] Tags
    );
