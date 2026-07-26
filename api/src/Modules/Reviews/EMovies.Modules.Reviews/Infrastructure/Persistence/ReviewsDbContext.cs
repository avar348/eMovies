using EMovies.Modules.Reviews.Domain;
using Microsoft.EntityFrameworkCore;

namespace EMovies.Modules.Reviews.Infrastructure.Persistence;

public sealed class ReviewsDbContext(DbContextOptions<ReviewsDbContext> options)
    : DbContext(options)
{
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reviews");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewsDbContext).Assembly);
    }
}
