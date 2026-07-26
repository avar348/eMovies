using EMovies.Api.Configuration;
using Microsoft.OpenApi;

namespace EMovies.Api.OpenApi;

internal static class SwaggerExtensions
{
    public const string SecuritySchemeName = "Keycloak";

    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloak = configuration
            .GetRequiredSection(KeycloakOptions.SectionName)
            .Get<KeycloakOptions>()
            ?? throw new InvalidOperationException("Keycloak configuration is missing.");

        services.AddOptions<SwaggerOptions>()
            .Bind(configuration.GetRequiredSection(SwaggerOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ClientId),
                "Swagger:ClientId is required.")
            .ValidateOnStart();

        var authority = keycloak.Authority.TrimEnd('/');

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "eMovies API",
                Version = "v1",
                Description = "Movies and reviews modular-monolith API."
            });

            options.AddSecurityDefinition(
                SecuritySchemeName,
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Sign in through Keycloak using Authorization Code with PKCE.",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(
                                $"{authority}/protocol/openid-connect/auth"),
                            TokenUrl = new Uri(
                                $"{authority}/protocol/openid-connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "Authenticate with OpenID Connect",
                                ["profile"] = "Read the signed-in user's profile"
                            }
                        }
                    }
                });

            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        var swagger = app.Configuration
            .GetRequiredSection(SwaggerOptions.SectionName)
            .Get<SwaggerOptions>()
            ?? throw new InvalidOperationException("Swagger configuration is missing.");

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "eMovies API v1");
            options.DocumentTitle = "eMovies API";
            options.OAuthClientId(swagger.ClientId);
            options.OAuthAppName("eMovies Swagger UI");
            options.OAuthScopes("openid", "profile");
            options.OAuthUsePkce();
        });

        return app;
    }
}
