using System.ComponentModel.DataAnnotations;

namespace EMovies.Modules.Reviews.Application.Models;

public sealed record SubmitReviewRequest(
    [property: Range(1, 5)] int Rating,
    [property: MaxLength(4_000)] string? Content);
