using EMovies.Modules.Users.Domain;

namespace EMovies.Modules.Users.Tests.Domain;

public sealed class UserProfileTests
{
    private static readonly TimeProvider TimeProvider =
        new FixedTimeProvider(new DateTimeOffset(2026, 7, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_MovieRenter_ActivatesProfile()
    {
        var profile = UserProfile.Create(
            "keycloak-subject",
            "viewer@example.test",
            "Movie Viewer",
            UserAccountType.MovieRenter,
            TimeProvider);

        Assert.Equal(UserOnboardingStatus.Active, profile.OnboardingStatus);
    }

    [Fact]
    public void Create_LibraryManager_RequiresApproval()
    {
        var profile = UserProfile.Create(
            "keycloak-subject",
            "manager@example.test",
            "Library Manager",
            UserAccountType.LibraryManager,
            TimeProvider);

        Assert.Equal(
            UserOnboardingStatus.PendingApproval,
            profile.OnboardingStatus);
    }

    [Fact]
    public void ApproveManagerAccess_ActivatesPendingManager()
    {
        var profile = UserProfile.Create(
            "keycloak-subject",
            "manager@example.test",
            "Library Manager",
            UserAccountType.LibraryManager,
            TimeProvider);

        profile.ApproveManagerAccess(TimeProvider);

        Assert.Equal(UserOnboardingStatus.Active, profile.OnboardingStatus);
    }

    [Fact]
    public void DenyManagerAccess_DeniesPendingManager()
    {
        var profile = UserProfile.Create(
            "keycloak-subject",
            "manager@example.test",
            "Library Manager",
            UserAccountType.LibraryManager,
            TimeProvider);

        profile.DenyManagerAccess(TimeProvider);

        Assert.Equal(UserOnboardingStatus.Denied, profile.OnboardingStatus);
    }

    [Fact]
    public void ApproveManagerAccess_RejectsActiveRenter()
    {
        var profile = UserProfile.Create(
            "keycloak-subject",
            "viewer@example.test",
            "Movie Viewer",
            UserAccountType.MovieRenter,
            TimeProvider);

        Assert.Throws<InvalidOperationException>(
            () => profile.ApproveManagerAccess(TimeProvider));
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
