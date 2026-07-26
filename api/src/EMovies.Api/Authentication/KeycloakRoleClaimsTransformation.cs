using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using EMovies.Api.Configuration;

namespace EMovies.Api.Authentication;

internal sealed class KeycloakRoleClaimsTransformation(
    IOptions<KeycloakOptions> keycloakOptions) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        AddRealmRoles(identity);
        AddClientRoles(identity);

        return Task.FromResult(principal);
    }

    private static void AddRealmRoles(ClaimsIdentity identity)
    {
        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return;
        }

        using var document = JsonDocument.Parse(realmAccess);
        if (!document.RootElement.TryGetProperty("roles", out var roles))
        {
            return;
        }

        AddRoles(identity, roles);
    }

    private void AddClientRoles(ClaimsIdentity identity)
    {
        var resourceAccess = identity.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
        {
            return;
        }

        using var document = JsonDocument.Parse(resourceAccess);
        if (document.RootElement.TryGetProperty(
                keycloakOptions.Value.Audience,
                out var client) &&
            client.TryGetProperty("roles", out var roles))
        {
            AddRoles(identity, roles);
        }
    }

    private static void AddRoles(ClaimsIdentity identity, JsonElement roles)
    {
        foreach (var role in roles.EnumerateArray())
        {
            var value = role.GetString();
            if (!string.IsNullOrWhiteSpace(value) &&
                !identity.HasClaim(ClaimTypes.Role, value))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, value));
            }
        }
    }
}
