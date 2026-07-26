using System.Text.Json.Serialization;
using EMovies.Api;
using EMovies.Api.Extensions;
using EMovies.Modules.Movies;
using EMovies.Modules.Movies.Presentation;
using EMovies.Modules.Reviews;
using EMovies.Modules.Reviews.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(MoviesPolicies.Read, policy =>
        policy.RequireRole(MoviesRoles.Reader, MoviesRoles.Admin))
    .AddPolicy(MoviesPolicies.Write, policy =>
        policy.RequireRole(MoviesRoles.Admin))
    .AddPolicy(ReviewsPolicies.Moderate, policy =>
        policy.RequireRole(ReviewsRoles.Moderator, MoviesRoles.Admin));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddApplicationPart(typeof(MoviesModule).Assembly)
    .AddApplicationPart(typeof(ReviewsModule).Assembly);

builder.Services.AddHealthChecks();
builder.Services.AddMoviesModule(builder.Configuration);
builder.Services.AddReviewsModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

await app.InitialiseMoviesModuleAsync();
await app.InitialiseReviewsModuleAsync();
await app.RunAsync();

public partial class Program;
