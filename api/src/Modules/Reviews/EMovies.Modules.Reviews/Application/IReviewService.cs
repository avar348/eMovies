using EMovies.Modules.Reviews.Application.Models;
using EMovies.Modules.Reviews.Domain;

namespace EMovies.Modules.Reviews.Application;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewResponse>?> GetApprovedForMovieAsync(
        Guid movieId,
        CancellationToken cancellationToken);

    Task<MovieRatingResponse?> GetMovieRatingAsync(
        Guid movieId,
        CancellationToken cancellationToken);

    Task<ReviewResponse?> SubmitAsync(
        Guid movieId,
        string userId,
        string userDisplayName,
        SubmitReviewRequest request,
        CancellationToken cancellationToken);

    Task<ReviewResponse?> UpdateOwnAsync(
        Guid movieId,
        string userId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteOwnAsync(
        Guid movieId,
        string userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ReviewResponse>> GetForModerationAsync(
        ReviewModerationStatus? status,
        CancellationToken cancellationToken);

    Task<ReviewResponse?> ModerateAsync(
        Guid reviewId,
        ModerateReviewRequest request,
        CancellationToken cancellationToken);
}
