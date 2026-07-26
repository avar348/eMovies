namespace EMovies.Modules.Users.Application;

public interface IIdentityRoleService
{
    Task AssignRealmRoleAsync(
        string identitySubject,
        string roleName,
        CancellationToken cancellationToken);

    Task RemoveRealmRoleAsync(
        string identitySubject,
        string roleName,
        CancellationToken cancellationToken);
}
