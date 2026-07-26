using System.Security.Claims;
using EMovies.Api.Authentication;
using EMovies.Api.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EMovies.Api.Extensions;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetRequiredSection(KeycloakOptions.SectionName)
            .Get<KeycloakOptions>()
            ?? throw new InvalidOperationException("Keycloak configuration is missing.");

        services.AddOptions<KeycloakOptions>()
            .Bind(configuration.GetRequiredSection(KeycloakOptions.SectionName))
            .Validate(
                keycloak => Uri.TryCreate(
                    keycloak.Authority,
                    UriKind.Absolute,
                    out _),
                "Keycloak:Authority must be an absolute URI.")
            .Validate(
                keycloak => !string.IsNullOrWhiteSpace(keycloak.Audience),
                "Keycloak:Audience is required.")
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.Authority = options.Authority;
                jwt.Audience = options.Audience;
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = options.Authority.TrimEnd('/'),
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddTransient<IClaimsTransformation, KeycloakRoleClaimsTransformation>();

        return services;
    }
}
