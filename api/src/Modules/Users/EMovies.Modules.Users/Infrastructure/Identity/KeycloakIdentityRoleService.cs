using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EMovies.Modules.Users.Application;
using Microsoft.Extensions.Options;

namespace EMovies.Modules.Users.Infrastructure.Identity;

internal sealed class KeycloakIdentityRoleService(
    HttpClient httpClient,
    IOptions<KeycloakAdminOptions> options) : IIdentityRoleService
{
    public async Task AssignRealmRoleAsync(
        string identitySubject,
        string roleName,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(roleName, cancellationToken);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            RealmAdminPath(
                $"users/{Uri.EscapeDataString(identitySubject)}/role-mappings/realm"))
        {
            Content = JsonContent.Create(new[] { role })
        };

        await SendAdminRequestAsync(request, cancellationToken);
    }

    public async Task RemoveRealmRoleAsync(
        string identitySubject,
        string roleName,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(roleName, cancellationToken);
        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            RealmAdminPath(
                $"users/{Uri.EscapeDataString(identitySubject)}/role-mappings/realm"))
        {
            Content = JsonContent.Create(new[] { role })
        };

        await SendAdminRequestAsync(request, cancellationToken);
    }

    private async Task<RoleRepresentation> GetRoleAsync(
        string roleName,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            RealmAdminPath($"roles/{Uri.EscapeDataString(roleName)}"));
        var response = await SendAdminRequestAsync(request, cancellationToken);
        var role = await response.Content.ReadFromJsonAsync<RoleRepresentation>(
            cancellationToken);

        return role ?? throw new InvalidOperationException(
            $"Keycloak role '{roleName}' was not found.");
    }

    private async Task<HttpResponseMessage> SendAdminRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await GetAdminAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken);

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var responseText = await response.Content.ReadAsStringAsync(
            cancellationToken);
        throw new InvalidOperationException(
            $"Keycloak admin request failed with {(int)response.StatusCode}: {responseText}");
    }

    private async Task<string> GetAdminAccessTokenAsync(
        CancellationToken cancellationToken)
    {
        var admin = options.Value.Admin;
        var values = new Dictionary<string, string>
        {
            ["client_id"] = Required(admin.ClientId, "Keycloak:Admin:ClientId")
        };

        if (!string.IsNullOrWhiteSpace(admin.ClientSecret))
        {
            values["grant_type"] = "client_credentials";
            values["client_secret"] = admin.ClientSecret;
        }
        else
        {
            values["grant_type"] = "password";
            values["username"] = Required(admin.Username, "Keycloak:Admin:Username");
            values["password"] = Required(admin.Password, "Keycloak:Admin:Password");
        }

        using var content = new FormUrlEncodedContent(values);
        var response = await httpClient.PostAsync(
            TokenPath(admin.Realm),
            content,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync(
                cancellationToken);
            throw new InvalidOperationException(
                $"Keycloak admin authentication failed with {(int)response.StatusCode}: {responseText}");
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(
            cancellationToken);
        return Required(token?.AccessToken, "Keycloak admin access token");
    }

    private string RealmAdminPath(string path)
    {
        var realm = RealmFromAuthority(options.Value.Authority);
        return $"admin/realms/{Uri.EscapeDataString(realm)}/{path}";
    }

    private string TokenPath(string realm)
    {
        return $"realms/{Uri.EscapeDataString(realm)}/protocol/openid-connect/token";
    }

    private static string RealmFromAuthority(string authority)
    {
        var segments = new Uri(authority).Segments
            .Select(segment => segment.Trim('/'))
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToArray();
        var realmIndex = Array.FindIndex(
            segments,
            segment => string.Equals(
                segment,
                "realms",
                StringComparison.OrdinalIgnoreCase));

        if (realmIndex < 0 || realmIndex == segments.Length - 1)
        {
            throw new InvalidOperationException(
                "Keycloak:Authority must include a realm path.");
        }

        return segments[realmIndex + 1];
    }

    public static Uri ResolveAdminBaseAddress(KeycloakAdminOptions options)
    {
        var baseUri = !string.IsNullOrWhiteSpace(options.MetadataAddress)
            ? new Uri(options.MetadataAddress)
            : new Uri(options.Authority);
        var marker = "/realms/";
        var markerIndex = baseUri.AbsoluteUri.IndexOf(
            marker,
            StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            throw new InvalidOperationException(
                "Keycloak authority or metadata address must include '/realms/'.");
        }

        return new Uri(baseUri.AbsoluteUri[..(markerIndex + 1)]);
    }

    private static string Required(string? value, string configurationKey)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"{configurationKey} is required for Keycloak role management.");
        }

        return value;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken);

    private sealed record RoleRepresentation(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name);
}
