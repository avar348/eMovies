using System.ComponentModel.DataAnnotations;

namespace EMovies.Modules.Movies.Application.Models;

public sealed record UpdateMovieRequest(
    [property: Required, MaxLength(200)] string Title,
    [property: MaxLength(2_000)] string? Description,
    DateOnly ReleaseDate,
    [property: Required, MaxLength(100)] string Genre);
