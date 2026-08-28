using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class TripIncidentConfiguration : IEntityTypeConfiguration<TripIncident>
{
    public void Configure(EntityTypeBuilder<TripIncident> builder)
    {
        builder.ToTable("trip_incidents");
        builder.HasKey(incident => incident.Id);
        builder.Property(incident => incident.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(incident => incident.Description).HasMaxLength(TripErrors.IncidentDescriptionMaxLength).IsRequired();
        builder.Property(incident => incident.IncidentAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(incident => incident.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(incident => new { incident.TripId, incident.IncidentAtUtc });
        builder.HasOne<Employee>().WithMany()
            .HasForeignKey(incident => new { incident.CompanyId, incident.ReportedByEmployeeId })
            .HasPrincipalKey(employee => new { employee.CompanyId, employee.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(incident => incident.DomainEvents);
    }
}
