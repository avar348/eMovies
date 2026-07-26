using EMovies.Modules.Reviews.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EMovies.Modules.Reviews.Infrastructure.Health;

internal sealed class ReviewsDatabaseHealthCheck(
    ReviewsDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Cannot connect to the Reviews database.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Cannot connect to the Reviews database.",
                exception);
        }
    }
}
