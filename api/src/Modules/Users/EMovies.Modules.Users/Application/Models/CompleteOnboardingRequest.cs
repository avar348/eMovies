using EMovies.Modules.Users.Domain;

namespace EMovies.Modules.Users.Application.Models;

public sealed record CompleteOnboardingRequest(UserAccountType AccountType);
