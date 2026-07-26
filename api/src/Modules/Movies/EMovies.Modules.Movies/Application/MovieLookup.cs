using EMovies.Modules.Movies.Contracts;
using EMovies.Modules.Movies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMovies.Modules.Movies.Application;

internal sealed class MovieLookup(MoviesDbContext dbContext) : IMovieLookup
{
    public Task<bool> ExistsAsync(Guid movieId, CancellationToken cancellationToken)
    {
        return dbContext.Movies
            .AsNoTracking()
            .AnyAsync(movie => movie.Id == movieId, cancellationToken);
    }
}
