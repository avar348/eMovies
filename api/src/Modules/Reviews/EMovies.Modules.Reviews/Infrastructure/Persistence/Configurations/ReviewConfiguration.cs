using EMovies.Modules.Reviews.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMovies.Modules.Reviews.Infrastructure.Persistence.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(review => review.Id);

        builder.Property(review => review.UserId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(review => review.UserDisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(review => review.Content)
            .HasMaxLength(4_000);

        builder.Property(review => review.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(review => review.ModerationReason)
            .HasMaxLength(1_000);

        builder.Property(review => review.CreatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(review => review.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(review => review.ModeratedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(review => new { review.MovieId, review.UserId })
            .IsUnique();

        builder.HasIndex(review => new { review.MovieId, review.Status });

        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_reviews_rating",
                "\"Rating\" BETWEEN 1 AND 5"));
    }
}
