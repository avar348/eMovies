using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EMovies.Api.OpenApi;

internal sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodAttributes = context.MethodInfo.GetCustomAttributes(true);
        var controllerAttributes =
            context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? [];

        if (methodAttributes.OfType<IAllowAnonymous>().Any() ||
            controllerAttributes.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        if (!methodAttributes.OfType<IAuthorizeData>().Any() &&
            !controllerAttributes.OfType<IAuthorizeData>().Any())
        {
            return;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                SwaggerExtensions.SecuritySchemeName,
                context.Document)] = ["openid", "profile"]
        });
    }
}
