using System.Text.Json.Serialization;
using EMovies.Api;
using EMovies.Api.Extensions;
using EMovies.Api.OpenApi;
using EMovies.Modules.Movies;
using EMovies.Modules.Movies.Presentation;
using EMovies.Modules.Reviews;
using EMovies.Modules.Reviews.Presentation;
using EMovies.Modules.Users;
using EMovies.Modules.Users.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(MoviesPolicies.Read, policy =>
        policy.RequireRole(
            MoviesRoles.Reader,
            MoviesRoles.Manager,
            MoviesRoles.Admin,
            MoviesRoles.LegacyReader,
            MoviesRoles.LegacyAdmin))
    .AddPolicy(MoviesPolicies.Write, policy =>
        policy.RequireRole(
            MoviesRoles.Manager,
            MoviesRoles.Admin,
            MoviesRoles.LegacyAdmin))
    .AddPolicy(ReviewsPolicies.Moderate, policy =>
        policy.RequireRole(
            ReviewsRoles.Moderator,
            ReviewsRoles.Manager,
            ReviewsRoles.LegacyModerator,
            MoviesRoles.Admin,
            MoviesRoles.LegacyAdmin))
    .AddPolicy(UsersPolicies.ManageApprovals, policy =>
        policy.RequireRole(UsersRoles.Admin, UsersRoles.LegacyAdmin));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddApplicationPart(typeof(MoviesModule).Assembly)
    .AddApplicationPart(typeof(ReviewsModule).Assembly)
    .AddApplicationPart(typeof(UsersModule).Assembly);

builder.Services.AddHealthChecks();
builder.Services.AddMoviesModule(builder.Configuration);
builder.Services.AddReviewsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

await app.InitialiseMoviesModuleAsync();
await app.InitialiseReviewsModuleAsync();
await app.InitialiseUsersModuleAsync();
await app.RunAsync();

public partial class Program;
