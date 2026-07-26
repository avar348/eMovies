using EMovies.Modules.Users.Application;
using EMovies.Modules.Users.Infrastructure.Health;
using EMovies.Modules.Users.Infrastructure.Identity;
using EMovies.Modules.Users.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EMovies.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'UsersDatabase' is missing.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "users")));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddOptions<KeycloakAdminOptions>()
            .Bind(configuration.GetRequiredSection(KeycloakAdminOptions.SectionName));
        services.AddHttpClient<IIdentityRoleService, KeycloakIdentityRoleService>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<
                        Microsoft.Extensions.Options.IOptions<KeycloakAdminOptions>>()
                    .Value;
                client.BaseAddress =
                    KeycloakIdentityRoleService.ResolveAdminBaseAddress(options);
            });
        services.AddHealthChecks()
            .AddCheck<UsersDatabaseHealthCheck>("users-database");

        return services;
    }

    public static async Task InitialiseUsersModuleAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Users:MigrateOnStartup"))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
