using EMovies.Modules.Reviews.Domain;

namespace EMovies.Modules.Reviews.Tests.Domain;

public sealed class ReviewTests
{
    [Fact]
    public void Create_StartsPendingAndTrimsWrittenReview()
    {
        var now = new DateTimeOffset(2026, 7, 26, 12, 0, 0, TimeSpan.Zero);
        var review = Review.Create(
            Guid.NewGuid(),
            "user-1",
            "  Reviewer  ",
            5,
            "  Excellent movie.  ",
            new FixedTimeProvider(now));

        Assert.Equal(5, review.Rating);
        Assert.Equal("Excellent movie.", review.Content);
        Assert.Equal("Reviewer", review.UserDisplayName);
        Assert.Equal(ReviewModerationStatus.Pending, review.Status);
        Assert.Equal(now.UtcDateTime, review.CreatedAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_RejectsRatingOutsideOneToFive(int rating)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Review.Create(
                Guid.NewGuid(),
                "user-1",
                "Reviewer",
                rating,
                null,
                TimeProvider.System));
    }

    [Fact]
    public void Update_ReturnsApprovedReviewToPendingModeration()
    {
        var clock = new MutableTimeProvider(
            new DateTimeOffset(2026, 7, 26, 12, 0, 0, TimeSpan.Zero));
        var review = Review.Create(
            Guid.NewGuid(),
            "user-1",
            "Reviewer",
            4,
            "Good",
            clock);

        review.Moderate(true, null, clock);
        clock.UtcNow = clock.UtcNow.AddHours(1);
        review.Update(5, "Great", clock);

        Assert.Equal(ReviewModerationStatus.Pending, review.Status);
        Assert.Null(review.ModeratedAtUtc);
        Assert.Equal("Great", review.Content);
    }

    [Fact]
    public void Moderate_RequiresReasonWhenRejected()
    {
        var review = Review.Create(
            Guid.NewGuid(),
            "user-1",
            "Reviewer",
            4,
            null,
            TimeProvider.System);

        Assert.Throws<ArgumentException>(() =>
            review.Moderate(false, " ", TimeProvider.System));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MutableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
