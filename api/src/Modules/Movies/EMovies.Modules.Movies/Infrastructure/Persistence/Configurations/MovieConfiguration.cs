using EMovies.Modules.Movies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMovies.Modules.Movies.Infrastructure.Persistence.Configurations;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("movies");
        builder.HasKey(movie => movie.Id);

        builder.Property(movie => movie.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(movie => movie.Description)
            .HasMaxLength(2_000);

        builder.Property(movie => movie.Genre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(movie => movie.ReleaseDate)
            .HasColumnType("date");

        builder.Property(movie => movie.CreatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(movie => movie.Title);
    }
}
