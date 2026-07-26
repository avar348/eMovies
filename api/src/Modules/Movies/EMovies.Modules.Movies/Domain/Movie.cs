namespace EMovies.Modules.Movies.Domain;

public sealed class Movie
{
    private Movie()
    {
    }

    private Movie(
        Guid id,
        string title,
        string? description,
        DateOnly releaseDate,
        string genre,
        DateTime createdAtUtc)
    {
        Id = id;
        Update(title, description, releaseDate, genre);
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateOnly ReleaseDate { get; private set; }

    public string Genre { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static Movie Create(
        string title,
        string? description,
        DateOnly releaseDate,
        string genre,
        TimeProvider timeProvider)
    {
        return new Movie(
            Guid.NewGuid(),
            title,
            description,
            releaseDate,
            genre,
            timeProvider.GetUtcNow().UtcDateTime);
    }

    public void Update(
        string title,
        string? description,
        DateOnly releaseDate,
        string genre)
    {
        Title = Required(title, nameof(title), 200);
        Genre = Required(genre, nameof(genre), 100);
        Description = Optional(description, nameof(description), 2_000);
        ReleaseDate = releaseDate;
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
