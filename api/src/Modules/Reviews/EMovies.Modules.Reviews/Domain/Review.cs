namespace EMovies.Modules.Reviews.Domain;

public sealed class Review
{
    private Review()
    {
    }

    private Review(
        Guid id,
        Guid movieId,
        string userId,
        string userDisplayName,
        int rating,
        string? content,
        DateTime createdAtUtc)
    {
        Id = id;
        MovieId = movieId;
        UserId = Required(userId, nameof(userId), 200);
        UserDisplayName = Required(userDisplayName, nameof(userDisplayName), 200);
        Rating = ValidateRating(rating);
        Content = Optional(content, nameof(content), 4_000);
        Status = ReviewModerationStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid MovieId { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public string UserDisplayName { get; private set; } = string.Empty;

    public int Rating { get; private set; }

    public string? Content { get; private set; }

    public ReviewModerationStatus Status { get; private set; }

    public string? ModerationReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? ModeratedAtUtc { get; private set; }

    public static Review Create(
        Guid movieId,
        string userId,
        string userDisplayName,
        int rating,
        string? content,
        TimeProvider timeProvider)
    {
        if (movieId == Guid.Empty)
        {
            throw new ArgumentException("A movie id is required.", nameof(movieId));
        }

        return new Review(
            Guid.NewGuid(),
            movieId,
            userId,
            userDisplayName,
            rating,
            content,
            timeProvider.GetUtcNow().UtcDateTime);
    }

    public void Update(int rating, string? content, TimeProvider timeProvider)
    {
        Rating = ValidateRating(rating);
        Content = Optional(content, nameof(content), 4_000);
        Status = ReviewModerationStatus.Pending;
        ModerationReason = null;
        ModeratedAtUtc = null;
        UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Moderate(
        bool approve,
        string? reason,
        TimeProvider timeProvider)
    {
        if (!approve && string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "A reason is required when rejecting a review.",
                nameof(reason));
        }

        Status = approve
            ? ReviewModerationStatus.Approved
            : ReviewModerationStatus.Rejected;
        ModerationReason = approve
            ? null
            : Optional(reason, nameof(reason), 1_000);
        ModeratedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        UpdatedAtUtc = ModeratedAtUtc.Value;
    }

    private static int ValidateRating(int rating)
    {
        return rating is >= 1 and <= 5
            ? rating
            : throw new ArgumentOutOfRangeException(
                nameof(rating),
                "Rating must be between 1 and 5.");
    }

    private static string Required(string value, string parameterName, int maximumLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException(
                $"The value cannot exceed {maximumLength} characters.",
                parameterName);
        }

        return trimmed;
    }

    private static string? Optional(string? value, string parameterName, int maximumLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException(
                $"The value cannot exceed {maximumLength} characters.",
                parameterName);
        }

        return trimmed;
    }
}
