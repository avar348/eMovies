using System.ComponentModel.DataAnnotations;

namespace EMovies.Modules.Reviews.Application.Models;

public sealed record ModerateReviewRequest(
    bool Approve,
    [property: MaxLength(1_000)] string? Reason);
