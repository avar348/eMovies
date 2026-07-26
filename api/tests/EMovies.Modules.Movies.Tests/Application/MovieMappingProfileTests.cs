using AutoMapper;
using EMovies.Modules.Movies.Application.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace EMovies.Modules.Movies.Tests.Application;

public sealed class MovieMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(
            config => config.AddProfile<MovieMappingProfile>(),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
