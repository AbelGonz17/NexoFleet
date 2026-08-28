using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Trips;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class TripReviewConfiguration : IEntityTypeConfiguration<TripReview>
{
    public void Configure(EntityTypeBuilder<TripReview> builder)
    {
        builder.ToTable("trip_reviews");
        builder.HasKey(review => review.Id);
        builder.Property(review => review.Decision).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(review => review.Comments).HasMaxLength(TripErrors.NotesMaxLength);
        builder.Property(review => review.ReviewedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(review => new { review.TripId, review.ReviewedAtUtc });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(review => review.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(review => review.DomainEvents);
    }
}
