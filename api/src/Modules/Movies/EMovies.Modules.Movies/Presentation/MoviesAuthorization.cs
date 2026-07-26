namespace EMovies.Modules.Movies.Presentation;

public static class MoviesPolicies
{
    public const string Read = "movies.read";
    public const string Write = "movies.write";
}

public static class MoviesRoles
{
    public const string Reader = "movies-reader";
    public const string Manager = "movies-manager";
    public const string Admin = "movies-admin";
    public const string LegacyReader = "emovies-member";
    public const string LegacyAdmin = "emovies-admin";
}
