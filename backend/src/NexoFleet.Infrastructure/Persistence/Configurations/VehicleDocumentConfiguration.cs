using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class VehicleDocumentConfiguration : IEntityTypeConfiguration<VehicleDocument>
{
    public void Configure(EntityTypeBuilder<VehicleDocument> builder)
    {
        builder.ToTable("vehicle_documents");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(document => document.FileName).HasMaxLength(VehicleErrors.DocumentFileNameMaxLength).IsRequired();
        builder.Property(document => document.StorageKey).HasMaxLength(VehicleErrors.DocumentStorageKeyMaxLength).IsRequired();
        builder.Property(document => document.ContentType).HasMaxLength(VehicleErrors.DocumentContentTypeMaxLength).IsRequired();
        builder.Property(document => document.ExpiresOn).HasColumnType("date");
        builder.Property(document => document.UploadedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(document => document.StorageKey).IsUnique();
        builder.HasIndex(document => new { document.VehicleId, document.Type, document.ExpiresOn });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(document => document.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(document => document.DomainEvents);
    }
}
