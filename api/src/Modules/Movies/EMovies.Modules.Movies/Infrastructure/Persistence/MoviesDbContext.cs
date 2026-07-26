using EMovies.Modules.Movies.Domain;
using Microsoft.EntityFrameworkCore;

namespace EMovies.Modules.Movies.Infrastructure.Persistence;

public sealed class MoviesDbContext(DbContextOptions<MoviesDbContext> options)
    : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("movies");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoviesDbContext).Assembly);
    }
}
