using EMovies.Modules.Reviews.Domain;

namespace EMovies.Modules.Reviews.Application.Models;

public sealed record ReviewResponse(
    Guid Id,
    Guid MovieId,
    string UserDisplayName,
    int Rating,
    string? Content,
    ReviewModerationStatus Status,
    string? ModerationReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ModeratedAtUtc);
