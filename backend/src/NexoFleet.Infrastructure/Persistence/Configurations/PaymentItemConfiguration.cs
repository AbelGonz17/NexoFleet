using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Payments;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class PaymentItemConfiguration : IEntityTypeConfiguration<PaymentItem>
{
    public void Configure(EntityTypeBuilder<PaymentItem> builder)
    {
        builder.ToTable("payment_items", table =>
            table.HasCheckConstraint("CK_payment_items_amount", "\"Amount\" >= 0"));
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Effect).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(PaymentErrors.DescriptionMaxLength).IsRequired();
        builder.Property(item => item.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(item => item.UpdatedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(item => item.PaymentReportId);
        builder.HasIndex(item => item.TripId);
        builder.HasOne<Trip>().WithMany()
            .HasForeignKey(item => new { item.CompanyId, item.TripId })
            .HasPrincipalKey(trip => new { trip.CompanyId, trip.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(item => item.DomainEvents);
    }
}
