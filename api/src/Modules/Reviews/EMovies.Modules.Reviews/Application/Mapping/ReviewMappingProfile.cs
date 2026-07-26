using AutoMapper;
using EMovies.Modules.Reviews.Application.Models;
using EMovies.Modules.Reviews.Domain;

namespace EMovies.Modules.Reviews.Application.Mapping;

public sealed class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        CreateMap<Review, ReviewResponse>();
    }
}
