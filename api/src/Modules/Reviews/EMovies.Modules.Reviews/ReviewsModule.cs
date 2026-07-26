using EMovies.Modules.Reviews.Application;
using EMovies.Modules.Reviews.Application.Mapping;
using EMovies.Modules.Reviews.Infrastructure.Health;
using EMovies.Modules.Reviews.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EMovies.Modules.Reviews;

public static class ReviewsModule
{
    public static IServiceCollection AddReviewsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReviewsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'ReviewsDatabase' is missing.");

        services.AddDbContext<ReviewsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "reviews")));

        services.AddAutoMapper(
            mapper =>
            {
                var licenseKey = configuration["AutoMapper:LicenseKey"];
                if (!string.IsNullOrWhiteSpace(licenseKey))
                {
                    mapper.LicenseKey = licenseKey;
                }
            },
            typeof(ReviewMappingProfile).Assembly);

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IReviewService, ReviewService>();
        services.AddHealthChecks()
            .AddCheck<ReviewsDatabaseHealthCheck>("reviews-database");

        return services;
    }

    public static async Task InitialiseReviewsModuleAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Reviews:MigrateOnStartup"))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReviewsDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
