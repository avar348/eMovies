using System.Security.Claims;
using EMovies.Modules.Reviews.Application;
using EMovies.Modules.Reviews.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMovies.Modules.Reviews.Presentation.Controllers;

[ApiController]
[Route("api/movies/{movieId:guid}")]
[Authorize]
public sealed class MovieReviewsController(IReviewService reviewService) : ControllerBase
{
    [HttpGet("reviews")]
    [ProducesResponseType<IReadOnlyList<ReviewResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetReviews(
        Guid movieId,
        CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetApprovedForMovieAsync(
            movieId,
            cancellationToken);

        return reviews is null ? NotFound() : Ok(reviews);
    }

    [HttpGet("rating")]
    [ProducesResponseType<MovieRatingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieRatingResponse>> GetRating(
        Guid movieId,
        CancellationToken cancellationToken)
    {
        var rating = await reviewService.GetMovieRatingAsync(
            movieId,
            cancellationToken);

        return rating is null ? NotFound() : Ok(rating);
    }

    [HttpPost("reviews")]
    [ProducesResponseType<ReviewResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponse>> Submit(
        Guid movieId,
        SubmitReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredClaim("sub");
        var userDisplayName =
            User.FindFirstValue("preferred_username") ??
            User.FindFirstValue(ClaimTypes.Name) ??
            userId;

        var review = await reviewService.SubmitAsync(
            movieId,
            userId,
            userDisplayName,
            request,
            cancellationToken);

        return review is null
            ? NotFound()
            : Created($"/api/movies/{movieId}/reviews", review);
    }

    [HttpPut("reviews/me")]
    [ProducesResponseType<ReviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponse>> UpdateOwn(
        Guid movieId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var review = await reviewService.UpdateOwnAsync(
            movieId,
            GetRequiredClaim("sub"),
            request,
            cancellationToken);

        return review is null ? NotFound() : Ok(review);
    }

    [HttpDelete("reviews/me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOwn(
        Guid movieId,
        CancellationToken cancellationToken)
    {
        return await reviewService.DeleteOwnAsync(
            movieId,
            GetRequiredClaim("sub"),
            cancellationToken)
            ? NoContent()
            : NotFound();
    }

    private string GetRequiredClaim(string claimType)
    {
        return User.FindFirstValue(claimType)
            ?? throw new InvalidOperationException(
                $"The authenticated user is missing the '{claimType}' claim.");
    }
}
