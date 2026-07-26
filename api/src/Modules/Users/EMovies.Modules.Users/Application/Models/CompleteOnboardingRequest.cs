using EMovies.Modules.Users.Domain;

namespace EMovies.Modules.Users.Application.Models;

public sealed record CompleteOnboardingRequest(
    UserAccountType AccountType,
    string? DisplayName = null,
    string? PhoneNumber = null,
    string? OrganizationName = null,
    string? AddressLine1 = null,
    string? AddressLine2 = null,
    string? City = null,
    string? StateRegion = null,
    string? PostalCode = null,
    string? Country = null,
    int? ServiceAreaMiles = null,
    string? ServiceAreaCoverage = null);
