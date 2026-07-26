using EMovies.Modules.Movies.Application.Models;

namespace EMovies.Modules.Movies.Application;

public interface IMovieService
{
    Task<IReadOnlyList<MovieResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<MovieResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<MovieResponse> CreateAsync(
        CreateMovieRequest request,
        CancellationToken cancellationToken);

    Task<MovieResponse?> UpdateAsync(
        Guid id,
        UpdateMovieRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
