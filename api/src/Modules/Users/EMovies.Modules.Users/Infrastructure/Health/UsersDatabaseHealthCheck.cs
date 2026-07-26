using EMovies.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EMovies.Modules.Users.Infrastructure.Health;

internal sealed class UsersDatabaseHealthCheck(
    UsersDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Cannot connect to the Users database.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Cannot connect to the Users database.",
                exception);
        }
    }
}
