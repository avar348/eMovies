namespace EMovies.Modules.Movies.Contracts;

public interface IMovieLookup
{
    Task<bool> ExistsAsync(Guid movieId, CancellationToken cancellationToken);
}
