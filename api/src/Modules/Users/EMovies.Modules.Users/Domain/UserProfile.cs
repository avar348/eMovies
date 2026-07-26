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

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static UserProfile Create(
        string identitySubject,
        string email,
        string displayName,
        UserAccountType accountType,
        TimeProvider timeProvider)
    {
        return new UserProfile(
            Guid.NewGuid(),
            identitySubject,
            email,
            displayName,
            accountType,
            timeProvider.GetUtcNow().UtcDateTime);
    }

    public void CompleteOnboarding(
        string email,
        string displayName,
        UserAccountType accountType,
        TimeProvider timeProvider)
    {
        Email = Required(email, nameof(email), 320);
        DisplayName = Required(displayName, nameof(displayName), 200);
        AccountType = accountType;
        OnboardingStatus = StatusFor(accountType);
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
}
