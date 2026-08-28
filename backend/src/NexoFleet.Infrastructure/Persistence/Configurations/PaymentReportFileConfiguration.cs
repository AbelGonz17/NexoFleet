using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Payments;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class PaymentReportFileConfiguration : IEntityTypeConfiguration<PaymentReportFile>
{
    public void Configure(EntityTypeBuilder<PaymentReportFile> builder)
    {
        builder.ToTable("payment_report_files");
        builder.HasKey(file => file.Id);
        builder.Property(file => file.FileName).HasMaxLength(PaymentErrors.FileNameMaxLength).IsRequired();
        builder.Property(file => file.StorageKey).HasMaxLength(PaymentErrors.StorageKeyMaxLength).IsRequired();
        builder.Property(file => file.ContentType).HasMaxLength(PaymentErrors.ContentTypeMaxLength).IsRequired();
        builder.Property(file => file.UploadedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(file => file.StorageKey).IsUnique();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(file => file.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(file => file.DomainEvents);
    }
}
