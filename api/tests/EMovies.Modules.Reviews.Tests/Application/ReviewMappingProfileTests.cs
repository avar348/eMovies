using AutoMapper;
using EMovies.Modules.Reviews.Application.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace EMovies.Modules.Reviews.Tests.Application;

public sealed class ReviewMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(
            config => config.AddProfile<ReviewMappingProfile>(),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
