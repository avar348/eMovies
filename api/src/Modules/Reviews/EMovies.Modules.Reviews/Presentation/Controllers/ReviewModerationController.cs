using EMovies.Modules.Reviews.Application;
using EMovies.Modules.Reviews.Application.Models;
using EMovies.Modules.Reviews.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMovies.Modules.Reviews.Presentation.Controllers;

[ApiController]
[Route("api/reviews/moderation")]
[Authorize(Policy = ReviewsPolicies.Moderate)]
public sealed class ReviewModerationController(IReviewService reviewService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ReviewResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetQueue(
        [FromQuery] ReviewModerationStatus? status = ReviewModerationStatus.Pending,
        CancellationToken cancellationToken = default)
    {
        return Ok(await reviewService.GetForModerationAsync(
            status,
            cancellationToken));
    }

    [HttpPatch("{reviewId:guid}")]
    [ProducesResponseType<ReviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponse>> Moderate(
        Guid reviewId,
        ModerateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var review = await reviewService.ModerateAsync(
            reviewId,
            request,
            cancellationToken);

        return review is null ? NotFound() : Ok(review);
    }
}
