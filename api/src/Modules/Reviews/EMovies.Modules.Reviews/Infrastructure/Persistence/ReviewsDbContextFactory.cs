using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EMovies.Modules.Reviews.Infrastructure.Persistence;

internal sealed class ReviewsDbContextFactory : IDesignTimeDbContextFactory<ReviewsDbContext>
{
    public ReviewsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__ReviewsDatabase")
            ?? "Host=localhost;Port=5432;Database=emovies;Username=emovies;Password=emovies_dev_password";

        var options = new DbContextOptionsBuilder<ReviewsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ReviewsDbContext(options);
    }
}
