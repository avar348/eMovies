using AutoMapper;
using AutoMapper.QueryableExtensions;
using EMovies.Modules.Movies.Contracts;
using EMovies.Modules.Reviews.Application.Models;
using EMovies.Modules.Reviews.Domain;
using EMovies.Modules.Reviews.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EMovies.Modules.Reviews.Application;

internal sealed class ReviewService(
    ReviewsDbContext dbContext,
    IMovieLookup movieLookup,
    IMapper mapper,
    TimeProvider timeProvider) : IReviewService
{
    public async Task<IReadOnlyList<ReviewResponse>?> GetApprovedForMovieAsync(
        Guid movieId,
        CancellationToken cancellationToken)
    {
        if (!await movieLookup.ExistsAsync(movieId, cancellationToken))
        {
            return null;
        }

        return await dbContext.Reviews
            .AsNoTracking()
            .Where(review =>
                review.MovieId == movieId &&
                review.Status == ReviewModerationStatus.Approved)
            .OrderByDescending(review => review.CreatedAtUtc)
            .ProjectTo<ReviewResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<MovieRatingResponse?> GetMovieRatingAsync(
        Guid movieId,
        CancellationToken cancellationToken)
    {
        if (!await movieLookup.ExistsAsync(movieId, cancellationToken))
        {
            return null;
        }

        var approvedReviews = dbContext.Reviews
            .AsNoTracking()
            .Where(review =>
                review.MovieId == movieId &&
                review.Status == ReviewModerationStatus.Approved);

        var reviewCount = await approvedReviews.CountAsync(cancellationToken);
        var averageRating = reviewCount == 0
            ? null
            : await approvedReviews.AverageAsync(
                review => (double?)review.Rating,
                cancellationToken);

        return new MovieRatingResponse(movieId, averageRating, reviewCount);
    }

    public async Task<ReviewResponse?> SubmitAsync(
        Guid movieId,
        string userId,
        string userDisplayName,
        SubmitReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!await movieLookup.ExistsAsync(movieId, cancellationToken))
        {
            return null;
        }

        if (await dbContext.Reviews.AnyAsync(
                review => review.MovieId == movieId && review.UserId == userId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "A user can submit only one review per movie.");
        }

        var review = Review.Create(
            movieId,
            userId,
            userDisplayName,
            request.Rating,
            request.Content,
            timeProvider);

        dbContext.Reviews.Add(review);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            throw new InvalidOperationException(
                "A user can submit only one review per movie.",
                exception);
        }

        return mapper.Map<ReviewResponse>(review);
    }

    public async Task<ReviewResponse?> UpdateOwnAsync(
        Guid movieId,
        string userId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var review = await dbContext.Reviews.SingleOrDefaultAsync(
            item => item.MovieId == movieId && item.UserId == userId,
            cancellationToken);

        if (review is null)
        {
            return null;
        }

        review.Update(request.Rating, request.Content, timeProvider);
        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<ReviewResponse>(review);
    }

    public async Task<bool> DeleteOwnAsync(
        Guid movieId,
        string userId,
        CancellationToken cancellationToken)
    {
        var review = await dbContext.Reviews.SingleOrDefaultAsync(
            item => item.MovieId == movieId && item.UserId == userId,
            cancellationToken);

        if (review is null)
        {
            return false;
        }

        dbContext.Reviews.Remove(review);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetForModerationAsync(
        ReviewModerationStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Reviews.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(review => review.Status == status.Value);
        }

        return await query
            .OrderBy(review => review.Status)
            .ThenBy(review => review.CreatedAtUtc)
            .ProjectTo<ReviewResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReviewResponse?> ModerateAsync(
        Guid reviewId,
        ModerateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var review = await dbContext.Reviews.FindAsync([reviewId], cancellationToken);
        if (review is null)
        {
            return null;
        }

        review.Moderate(request.Approve, request.Reason, timeProvider);
        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<ReviewResponse>(review);
    }
}
