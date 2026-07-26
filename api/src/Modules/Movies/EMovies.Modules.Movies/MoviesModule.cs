using EMovies.Modules.Movies.Application;
using EMovies.Modules.Movies.Application.Mapping;
using EMovies.Modules.Movies.Contracts;
using EMovies.Modules.Movies.Infrastructure.Health;
using EMovies.Modules.Movies.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EMovies.Modules.Movies;

public static class MoviesModule
{
    public static IServiceCollection AddMoviesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MoviesDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'MoviesDatabase' is missing.");

        services.AddDbContext<MoviesDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "movies")));

        services.AddAutoMapper(
            mapper =>
            {
                var licenseKey = configuration["AutoMapper:LicenseKey"];
                if (!string.IsNullOrWhiteSpace(licenseKey))
                {
                    mapper.LicenseKey = licenseKey;
                }
            },
            typeof(MovieMappingProfile).Assembly);

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IMovieLookup, MovieLookup>();
        services.AddHealthChecks()
            .AddCheck<MoviesDatabaseHealthCheck>("movies-database");

        return services;
    }

    public static async Task InitialiseMoviesModuleAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Movies:MigrateOnStartup"))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
