namespace EMovies.Modules.Movies.Application.Models;

public sealed record MovieResponse(
    Guid Id,
    string Title,
    string? Description,
    DateOnly ReleaseDate,
    string Genre,
    DateTime CreatedAtUtc);
