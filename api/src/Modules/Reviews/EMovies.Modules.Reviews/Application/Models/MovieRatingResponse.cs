namespace EMovies.Modules.Reviews.Application.Models;

public sealed record MovieRatingResponse(
    Guid MovieId,
    double? AverageRating,
    int ReviewCount);
