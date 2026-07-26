using EMovies.Modules.Reviews.Domain;
using EMovies.Modules.Reviews.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMovies.Modules.Reviews.Tests.Infrastructure;

public sealed class ReviewPersistenceModelTests
{
    [Fact]
    public void Model_EnforcesOneReviewPerUserPerMovie()
    {
        var options = new DbContextOptionsBuilder<ReviewsDbContext>()
            .UseNpgsql("Host=localhost;Database=emovies;Username=test;Password=test")
            .Options;

        using var dbContext = new ReviewsDbContext(options);
        var entity = dbContext.Model.FindEntityType(typeof(Review));
        var uniqueIndex = Assert.Single(
            entity!.GetIndexes(),
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(["MovieId", "UserId"]));

        Assert.True(uniqueIndex.IsUnique);
    }
}
