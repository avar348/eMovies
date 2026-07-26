using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EMovies.Modules.Users.Infrastructure.Persistence;

internal sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__UsersDatabase")
            ?? "Host=localhost;Port=5432;Database=emovies;Username=emovies;Password=emovies_dev_password";

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new UsersDbContext(options);
    }
}
