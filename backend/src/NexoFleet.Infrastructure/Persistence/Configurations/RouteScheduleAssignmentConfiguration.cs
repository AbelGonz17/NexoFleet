using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class RouteScheduleAssignmentConfiguration
    : IEntityTypeConfiguration<RouteScheduleAssignment>
{
    public void Configure(EntityTypeBuilder<RouteScheduleAssignment> builder)
    {
        builder.ToTable("route_schedule_assignments", table =>
            table.HasCheckConstraint(
                "ck_route_schedule_assignments_valid_period",
                "\"valid_until\" IS NULL OR \"valid_until\" >= \"valid_from\""));

        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.ValidFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(assignment => assignment.ValidUntil)
            .HasColumnType("date");

        builder.Property(assignment => assignment.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(assignment => assignment.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(assignment => new
        {
            assignment.RouteScheduleId,
            assignment.ValidFrom,
            assignment.ValidUntil
        }).HasDatabaseName("ix_route_schedule_assignments_period");

        builder.HasIndex(assignment => assignment.RouteScheduleId)
            .IsUnique()
            .HasFilter("\"valid_until\" IS NULL");

        builder.HasIndex(assignment => new
        {
            assignment.CompanyId,
            assignment.EmployeeId,
            assignment.ValidFrom
        });

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(assignment => new
            {
                assignment.CompanyId,
                assignment.EmployeeId
            })
            .HasPrincipalKey(employee => new { employee.CompanyId, employee.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(assignment => new
            {
                assignment.CompanyId,
                assignment.VehicleId
            })
            .HasPrincipalKey(vehicle => new { vehicle.CompanyId, vehicle.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(assignment => assignment.DomainEvents);
    }
}
