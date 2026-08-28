using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class RouteScheduleConfiguration
    : IEntityTypeConfiguration<RouteSchedule>
{
    public void Configure(EntityTypeBuilder<RouteSchedule> builder)
    {
        builder.ToTable("route_schedules", table =>
            table.HasCheckConstraint(
                "CK_route_schedules_effective_period",
                "\"EffectiveUntil\" IS NULL OR \"EffectiveUntil\" >= \"EffectiveFrom\""));

        builder.HasKey(schedule => schedule.Id);
        builder.HasAlternateKey(schedule => new { schedule.CompanyId, schedule.Id });

        builder.Property(schedule => schedule.Shift)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(schedule => schedule.StartTime)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(schedule => schedule.EndTime)
            .HasColumnType("time without time zone");

        builder.Property(schedule => schedule.EffectiveFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(schedule => schedule.EffectiveUntil)
            .HasColumnType("date");

        builder.Property(schedule => schedule.DefaultAmount)
            .HasPrecision(18, 2);

        builder.Property(schedule => schedule.DefaultCurrency)
            .HasMaxLength(RouteScheduleErrors.CurrencyLength);

        builder.Property(schedule => schedule.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(schedule => schedule.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(schedule => schedule.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(schedule => new
        {
            schedule.CompanyId,
            schedule.RouteId,
            schedule.Status
        });

        builder.HasIndex(schedule => new
        {
            schedule.CompanyId,
            schedule.EffectiveFrom,
            schedule.EffectiveUntil
        });

        builder.HasOne<Route>()
            .WithMany()
            .HasForeignKey(schedule => new { schedule.CompanyId, schedule.RouteId })
            .HasPrincipalKey(route => new { route.CompanyId, route.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(schedule => schedule.Days)
            .WithOne()
            .HasForeignKey(day => day.RouteScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(schedule => schedule.Assignments)
            .WithOne()
            .HasForeignKey(assignment => new
            {
                assignment.CompanyId,
                assignment.RouteScheduleId
            })
            .HasPrincipalKey(schedule => new { schedule.CompanyId, schedule.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(schedule => schedule.Days)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(schedule => schedule.Assignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(schedule => schedule.DomainEvents);
    }
}
