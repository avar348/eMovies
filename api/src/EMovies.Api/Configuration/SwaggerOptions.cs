namespace EMovies.Api.Configuration;

public sealed class SwaggerOptions
{
    public const string SectionName = "Swagger";

    public required string ClientId { get; init; }
}
