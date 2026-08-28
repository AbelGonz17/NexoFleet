using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class TripAssignmentConfiguration : IEntityTypeConfiguration<TripAssignment>
{
    public void Configure(EntityTypeBuilder<TripAssignment> builder)
    {
        builder.ToTable("trip_assignments");
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.AssignedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(assignment => assignment.EndedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(assignment => assignment.TripId).IsUnique().HasFilter("\"ended_at_utc\" IS NULL");
        builder.HasIndex(assignment => new { assignment.CompanyId, assignment.EmployeeId, assignment.AssignedAtUtc });

        builder.HasOne<Employee>().WithMany()
            .HasForeignKey(assignment => new { assignment.CompanyId, assignment.EmployeeId })
            .HasPrincipalKey(employee => new { employee.CompanyId, employee.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Vehicle>().WithMany()
            .HasForeignKey(assignment => new { assignment.CompanyId, assignment.VehicleId })
            .HasPrincipalKey(vehicle => new { vehicle.CompanyId, vehicle.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany()
            .HasForeignKey(assignment => assignment.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(assignment => assignment.DomainEvents);
    }
}
