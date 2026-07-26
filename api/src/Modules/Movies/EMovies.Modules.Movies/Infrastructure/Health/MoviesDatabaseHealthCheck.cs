using EMovies.Modules.Movies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EMovies.Modules.Movies.Infrastructure.Health;

internal sealed class MoviesDatabaseHealthCheck(
    MoviesDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Cannot connect to the Movies database.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Cannot connect to the Movies database.",
                exception);
        }
    }
}
