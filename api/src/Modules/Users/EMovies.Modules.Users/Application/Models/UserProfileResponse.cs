using EMovies.Modules.Users.Domain;

namespace EMovies.Modules.Users.Application.Models;

public sealed record UserProfileResponse(
    Guid Id,
    string IdentitySubject,
    string Email,
    string DisplayName,
    UserAccountType AccountType,
    UserOnboardingStatus OnboardingStatus,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
