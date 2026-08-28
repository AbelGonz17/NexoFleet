using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class TripStatusHistoryConfiguration : IEntityTypeConfiguration<TripStatusHistory>
{
    public void Configure(EntityTypeBuilder<TripStatusHistory> builder)
    {
        builder.ToTable("trip_status_history");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.PreviousStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(history => history.CurrentStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(history => history.Notes).HasMaxLength(TripErrors.NotesMaxLength);
        builder.Property(history => history.OccurredAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(history => new { history.TripId, history.OccurredAtUtc });
        builder.Ignore(history => history.DomainEvents);
    }
}
