using AutoMapper;
using EMovies.Modules.Movies.Application.Models;
using EMovies.Modules.Movies.Domain;

namespace EMovies.Modules.Movies.Application.Mapping;

public sealed class MovieMappingProfile : Profile
{
    public MovieMappingProfile()
    {
        CreateMap<Movie, MovieResponse>();
    }
}
