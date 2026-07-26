using EMovies.Modules.Users.Domain;

namespace EMovies.Modules.Users.Application.Models;

public sealed record UserProfileResponse(
    Guid Id,
    string IdentitySubject,
    string Email,
    string DisplayName,
    UserAccountType AccountType,
    UserOnboardingStatus OnboardingStatus,
    string? PhoneNumber,
    string? OrganizationName,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateRegion,
    string? PostalCode,
    string? Country,
    int? ServiceAreaMiles,
    string? ServiceAreaCoverage,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
