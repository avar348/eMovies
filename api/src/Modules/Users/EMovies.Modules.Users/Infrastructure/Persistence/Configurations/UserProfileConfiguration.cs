using EMovies.Modules.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMovies.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration
    : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.IdentitySubject)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.AccountType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(user => user.OnboardingStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(user => user.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(user => user.IdentitySubject)
            .IsUnique();
    }
}
