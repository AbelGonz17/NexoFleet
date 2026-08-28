using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class RouteScheduleDayConfiguration
    : IEntityTypeConfiguration<RouteScheduleDay>
{
    public void Configure(EntityTypeBuilder<RouteScheduleDay> builder)
    {
        builder.ToTable("route_schedule_days");

        builder.HasKey(day => new { day.RouteScheduleId, day.DayOfWeek });

        builder.Property(day => day.DayOfWeek)
            .HasConversion<string>()
            .HasMaxLength(9)
            .IsRequired();
    }
}
