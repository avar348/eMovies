using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EMovies.Modules.Movies.Infrastructure.Persistence;

internal sealed class MoviesDbContextFactory : IDesignTimeDbContextFactory<MoviesDbContext>
{
    public MoviesDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__MoviesDatabase")
            ?? "Host=localhost;Port=5432;Database=emovies;Username=emovies;Password=emovies_dev_password";

        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MoviesDbContext(options);
    }
}
