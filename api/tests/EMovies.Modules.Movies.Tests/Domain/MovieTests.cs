using EMovies.Modules.Movies.Domain;

namespace EMovies.Modules.Movies.Tests.Domain;

public sealed class MovieTests
{
    [Fact]
    public void Create_TrimsValuesAndSetsCreationTime()
    {
        var now = new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(now);

        var movie = Movie.Create(
            "  The Matrix  ",
            "  A simulation.  ",
            new DateOnly(1999, 3, 31),
            "  Science Fiction  ",
            timeProvider);

        Assert.Equal("The Matrix", movie.Title);
        Assert.Equal("A simulation.", movie.Description);
        Assert.Equal("Science Fiction", movie.Genre);
        Assert.Equal(now.UtcDateTime, movie.CreatedAtUtc);
    }

    [Fact]
    public void Create_RejectsBlankTitle()
    {
        Assert.Throws<ArgumentException>(() =>
            Movie.Create(
                " ",
                null,
                new DateOnly(2026, 1, 1),
                "Drama",
                TimeProvider.System));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
