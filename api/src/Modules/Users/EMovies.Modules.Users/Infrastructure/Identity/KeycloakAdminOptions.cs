namespace EMovies.Modules.Users.Infrastructure.Identity;

internal sealed class KeycloakAdminOptions
{
    public const string SectionName = "Keycloak";

    public required string Authority { get; init; }

    public string? MetadataAddress { get; init; }

    public AdminClientOptions Admin { get; init; } = new();
}

internal sealed class AdminClientOptions
{
    public string Realm { get; init; } = "master";

    public string ClientId { get; init; } = "admin-cli";

    public string? ClientSecret { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }
}
