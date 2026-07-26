namespace EMovies.Modules.Users.Domain;

public sealed class UserProfile
{
    private UserProfile()
    {
    }

    private UserProfile(
        Guid id,
        string identitySubject,
        string email,
        string displayName,
        UserAccountType accountType,
        DateTime createdAtUtc)
    {
        Id = id;
        IdentitySubject = Required(identitySubject, nameof(identitySubject), 200);
        Email = Required(email, nameof(email), 320);
        DisplayName = Required(displayName, nameof(displayName), 200);
        AccountType = accountType;
        OnboardingStatus = StatusFor(accountType);
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string IdentitySubject { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public UserAccountType AccountType { get; private set; }

    public UserOnboardingStatus OnboardingStatus { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? OrganizationName { get; private set; }

    public string? AddressLine1 { get; private set; }

    public string? AddressLine2 { get; private set; }

    public string? City { get; private set; }

    public string? StateRegion { get; private set; }

    public string? PostalCode { get; private set; }

    public string? Country { get; private set; }

    public int? ServiceAreaMiles { get; private set; }

    public string? ServiceAreaCoverage { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static UserProfile Create(
        string identitySubject,
        string email,
        string displayName,
        UserAccountType accountType,
        TimeProvider timeProvider,
        string? phoneNumber = null,
        string? organizationName = null,
        string? addressLine1 = null,
        string? addressLine2 = null,
        string? city = null,
        string? stateRegion = null,
        string? postalCode = null,
        string? country = null,
        int? serviceAreaMiles = null,
        string? serviceAreaCoverage = null)
    {
        return new UserProfile(
            Guid.NewGuid(),
            identitySubject,
            email,
            displayName,
            accountType,
            timeProvider.GetUtcNow().UtcDateTime)
            .ApplyProfileDetails(
                phoneNumber,
                organizationName,
                addressLine1,
                addressLine2,
                city,
                stateRegion,
                postalCode,
                country,
                serviceAreaMiles,
                serviceAreaCoverage);
    }

    public void CompleteOnboarding(
        string email,
        string displayName,
        UserAccountType accountType,
        TimeProvider timeProvider,
        string? phoneNumber = null,
        string? organizationName = null,
        string? addressLine1 = null,
        string? addressLine2 = null,
        string? city = null,
        string? stateRegion = null,
        string? postalCode = null,
        string? country = null,
        int? serviceAreaMiles = null,
        string? serviceAreaCoverage = null)
    {
        Email = Required(email, nameof(email), 320);
        DisplayName = Required(displayName, nameof(displayName), 200);
        AccountType = accountType;
        OnboardingStatus = StatusFor(accountType);
        ApplyProfileDetails(
            phoneNumber,
            organizationName,
            addressLine1,
            addressLine2,
            city,
            stateRegion,
            postalCode,
            country,
            serviceAreaMiles,
            serviceAreaCoverage);
        UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void ApproveManagerAccess(TimeProvider timeProvider)
    {
        EnsurePendingManagerApproval();
        OnboardingStatus = UserOnboardingStatus.Active;
        UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void DenyManagerAccess(TimeProvider timeProvider)
    {
        EnsurePendingManagerApproval();
        OnboardingStatus = UserOnboardingStatus.Denied;
        UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
    }

    private void EnsurePendingManagerApproval()
    {
        if (AccountType != UserAccountType.LibraryManager ||
            OnboardingStatus != UserOnboardingStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                "Only pending library manager profiles can be reviewed.");
        }
    }

    private static UserOnboardingStatus StatusFor(UserAccountType accountType)
    {
        return accountType == UserAccountType.LibraryManager
            ? UserOnboardingStatus.PendingApproval
            : UserOnboardingStatus.Active;
    }

    private UserProfile ApplyProfileDetails(
        string? phoneNumber,
        string? organizationName,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateRegion,
        string? postalCode,
        string? country,
        int? serviceAreaMiles,
        string? serviceAreaCoverage)
    {
        PhoneNumber = Optional(phoneNumber, nameof(phoneNumber), 40);
        AddressLine1 = Optional(addressLine1, nameof(addressLine1), 200);
        AddressLine2 = Optional(addressLine2, nameof(addressLine2), 200);
        City = Optional(city, nameof(city), 120);
        StateRegion = Optional(stateRegion, nameof(stateRegion), 120);
        PostalCode = Optional(postalCode, nameof(postalCode), 40);
        Country = Optional(country, nameof(country), 120);

        if (AccountType != UserAccountType.LibraryManager)
        {
            OrganizationName = null;
            ServiceAreaMiles = null;
            ServiceAreaCoverage = null;
            return this;
        }

        OrganizationName = Optional(organizationName, nameof(organizationName), 200);
        ServiceAreaMiles = serviceAreaMiles;
        ServiceAreaCoverage = Optional(serviceAreaCoverage, nameof(serviceAreaCoverage), 1_000);

        if (ServiceAreaMiles is < 0 or > 10_000)
        {
            throw new ArgumentException(
                "The service area distance must be between 0 and 10000 miles.",
                nameof(serviceAreaMiles));
        }

        return this;
    }

    private static string Required(
        string value,
        string parameterName,
        int maximumLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException(
                $"The value cannot exceed {maximumLength} characters.",
                parameterName);
        }

        return trimmed;
    }

    private static string? Optional(
        string? value,
        string parameterName,
        int maximumLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException(
                $"The value cannot exceed {maximumLength} characters.",
                parameterName);
        }

        return trimmed;
    }
}
