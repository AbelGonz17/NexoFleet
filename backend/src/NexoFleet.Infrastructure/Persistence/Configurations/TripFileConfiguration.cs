using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Trips;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class TripFileConfiguration : IEntityTypeConfiguration<TripFile>
{
    public void Configure(EntityTypeBuilder<TripFile> builder)
    {
        builder.ToTable("trip_files");
        builder.HasKey(file => file.Id);
        builder.Property(file => file.FileName).HasMaxLength(TripErrors.FileNameMaxLength).IsRequired();
        builder.Property(file => file.StorageKey).HasMaxLength(TripErrors.StorageKeyMaxLength).IsRequired();
        builder.Property(file => file.ContentType).HasMaxLength(TripErrors.ContentTypeMaxLength).IsRequired();
        builder.Property(file => file.UploadedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(file => file.StorageKey).IsUnique();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(file => file.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(file => file.DomainEvents);
    }
}
