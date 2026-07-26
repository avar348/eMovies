using EMovies.Modules.Users.Application.Models;
using EMovies.Modules.Users.Domain;

namespace EMovies.Modules.Users.Application;

public interface IUserProfileService
{
    Task<UserProfileResponse?> GetAsync(
        string identitySubject,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UserProfileResponse>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken);

    Task<UserProfileResponse?> ApproveManagerAsync(
        Guid profileId,
        CancellationToken cancellationToken);

    Task<UserProfileResponse?> DenyManagerAsync(
        Guid profileId,
        CancellationToken cancellationToken);

    Task<UserProfileResponse> CompleteOnboardingAsync(
        string identitySubject,
        string email,
        string displayName,
        UserAccountType accountType,
        CancellationToken cancellationToken);
}
