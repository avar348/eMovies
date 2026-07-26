using AutoMapper;
using AutoMapper.QueryableExtensions;
using EMovies.Modules.Movies.Application.Models;
using EMovies.Modules.Movies.Domain;
using EMovies.Modules.Movies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMovies.Modules.Movies.Application;

internal sealed class MovieService(
    MoviesDbContext dbContext,
    IMapper mapper,
    TimeProvider timeProvider) : IMovieService
{
    public async Task<IReadOnlyList<MovieResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Movies
            .AsNoTracking()
            .OrderBy(movie => movie.Title)
            .ProjectTo<MovieResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<MovieResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Movies
            .AsNoTracking()
            .Where(movie => movie.Id == id)
            .ProjectTo<MovieResponse>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<MovieResponse> CreateAsync(
        CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = Movie.Create(
            request.Title,
            request.Description,
            request.ReleaseDate,
            request.Genre,
            timeProvider);

        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<MovieResponse>(movie);
    }

    public async Task<MovieResponse?> UpdateAsync(
        Guid id,
        UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await dbContext.Movies.FindAsync([id], cancellationToken);
        if (movie is null)
        {
            return null;
        }

        movie.Update(
            request.Title,
            request.Description,
            request.ReleaseDate,
            request.Genre);

        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<MovieResponse>(movie);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = await dbContext.Movies.FindAsync([id], cancellationToken);
        if (movie is null)
        {
            return false;
        }

        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
