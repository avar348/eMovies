using EMovies.Modules.Users.Application.Models;
using EMovies.Modules.Users.Domain;
using EMovies.Modules.Users.Infrastructure.Persistence;
using EMovies.Modules.Users.Presentation;
using Microsoft.EntityFrameworkCore;

namespace EMovies.Modules.Users.Application;

internal sealed class UserProfileService(
    UsersDbContext dbContext,
    TimeProvider timeProvider,
    IIdentityRoleService identityRoleService) : IUserProfileService
{
    public async Task<UserProfileResponse?> GetAsync(
        string identitySubject,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.IdentitySubject == identitySubject,
                cancellationToken);

        return profile is null ? null : Map(profile);
    }

    public async Task<IReadOnlyList<UserProfileResponse>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken)
    {
        var profiles = await dbContext.UserProfiles
            .AsNoTracking()
            .Where(profile =>
                profile.AccountType == UserAccountType.LibraryManager &&
                profile.OnboardingStatus == UserOnboardingStatus.PendingApproval)
            .OrderBy(profile => profile.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return profiles.Select(Map).ToList();
    }

    public async Task<UserProfileResponse?> ApproveManagerAsync(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.UserProfiles.FindAsync(
            [profileId],
            cancellationToken);
        if (profile is null)
        {
            return null;
        }

        profile.ApproveManagerAccess(timeProvider);
        await identityRoleService.AssignRealmRoleAsync(
            profile.IdentitySubject,
            UsersRoles.Manager,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(profile);
    }

    public async Task<UserProfileResponse?> DenyManagerAsync(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.UserProfiles.FindAsync(
            [profileId],
            cancellationToken);
        if (profile is null)
        {
            return null;
        }

        profile.DenyManagerAccess(timeProvider);
        await identityRoleService.RemoveRealmRoleAsync(
            profile.IdentitySubject,
            UsersRoles.Manager,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(profile);
    }

    public async Task<UserProfileResponse> CompleteOnboardingAsync(
        string identitySubject,
        string email,
        string displayName,
        UserAccountType accountType,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.UserProfiles.SingleOrDefaultAsync(
            user => user.IdentitySubject == identitySubject,
            cancellationToken);

        if (profile is null)
        {
            profile = UserProfile.Create(
                identitySubject,
                email,
                displayName,
                accountType,
                timeProvider);
            dbContext.UserProfiles.Add(profile);
        }
        else
        {
            profile.CompleteOnboarding(
                email,
                displayName,
                accountType,
                timeProvider);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(profile);
    }

    private static UserProfileResponse Map(UserProfile profile)
    {
        return new UserProfileResponse(
            profile.Id,
            profile.IdentitySubject,
            profile.Email,
            profile.DisplayName,
            profile.AccountType,
            profile.OnboardingStatus,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);
    }
}
