using System.Security.Claims;
using EMovies.Modules.Users.Application;
using EMovies.Modules.Users.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMovies.Modules.Users.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    IUserProfileService userProfileService) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> GetMe(
        CancellationToken cancellationToken)
    {
        var profile = await userProfileService.GetAsync(
            GetRequiredClaim("sub"),
            cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("pending-approvals")]
    [Authorize(Policy = UsersPolicies.ManageApprovals)]
    [ProducesResponseType<IReadOnlyList<UserProfileResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserProfileResponse>>> GetPendingApprovals(
        CancellationToken cancellationToken)
    {
        return Ok(await userProfileService.GetPendingApprovalsAsync(
            cancellationToken));
    }

    [HttpPatch("pending-approvals/{profileId:guid}/approve")]
    [Authorize(Policy = UsersPolicies.ManageApprovals)]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileResponse>> ApproveManager(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var profile = await userProfileService.ApproveManagerAsync(
            profileId,
            cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPatch("pending-approvals/{profileId:guid}/deny")]
    [Authorize(Policy = UsersPolicies.ManageApprovals)]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileResponse>> DenyManager(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var profile = await userProfileService.DenyManagerAsync(
            profileId,
            cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me/onboarding")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileResponse>> CompleteOnboarding(
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken)
    {
        var identitySubject = GetRequiredClaim("sub");
        var email =
            User.FindFirstValue("email") ??
            $"{identitySubject}@identity.local";
        var displayName =
            request.DisplayName ??
            User.FindFirstValue("name") ??
            User.FindFirstValue("preferred_username") ??
            identitySubject;

        return Ok(await userProfileService.CompleteOnboardingAsync(
            identitySubject,
            email,
            displayName,
            request.AccountType,
            request.PhoneNumber,
            request.OrganizationName,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.StateRegion,
            request.PostalCode,
            request.Country,
            request.ServiceAreaMiles,
            request.ServiceAreaCoverage,
            cancellationToken));
    }

    private string GetRequiredClaim(string claimType)
    {
        return User.FindFirstValue(claimType)
            ?? throw new InvalidOperationException(
                $"The authenticated user is missing the '{claimType}' claim.");
    }
}
