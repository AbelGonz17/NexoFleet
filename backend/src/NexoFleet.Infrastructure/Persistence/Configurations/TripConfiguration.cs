using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips", table =>
        {
            table.HasCheckConstraint("CK_trips_agreed_amount", "\"AgreedAmount\" IS NULL OR \"AgreedAmount\" >= 0");
            table.HasCheckConstraint("CK_trips_final_amount", "\"FinalAmount\" IS NULL OR \"FinalAmount\" >= 0");
            table.HasCheckConstraint("CK_trips_service_times", "\"CompletedAtUtc\" IS NULL OR \"StartedAtUtc\" IS NULL OR \"CompletedAtUtc\" >= \"StartedAtUtc\"");
        });

        builder.HasKey(trip => trip.Id);
        builder.HasAlternateKey(trip => new { trip.CompanyId, trip.Id });
        builder.Property(trip => trip.TripNumber).HasMaxLength(TripErrors.TripNumberMaxLength).IsRequired();
        builder.Property(trip => trip.Source).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(trip => trip.ServiceDate).HasColumnType("date").IsRequired();

        ConfigureLocation(builder.ComplexProperty(trip => trip.Origin), "origin");
        ConfigureLocation(builder.ComplexProperty(trip => trip.Destination), "destination");

        builder.Property(trip => trip.AgreedAmount).HasPrecision(18, 2);
        builder.Property(trip => trip.FinalAmount).HasPrecision(18, 2);
        builder.Property(trip => trip.Currency).HasMaxLength(TripErrors.CurrencyLength);
        builder.Property(trip => trip.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(trip => trip.StartedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(trip => trip.CompletedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(trip => trip.CancellationReason).HasMaxLength(TripErrors.NotesMaxLength);
        builder.Property(trip => trip.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(trip => trip.UpdatedAtUtc).HasColumnType("timestamp with time zone");

        builder.HasIndex(trip => new { trip.CompanyId, trip.TripNumber }).IsUnique();
        builder.HasIndex(trip => new { trip.CompanyId, trip.ServiceDate, trip.Status });
        builder.HasIndex(trip => new { trip.CompanyId, trip.SubmittedByEmployeeId });

        builder.HasOne<Company>().WithMany().HasForeignKey(trip => trip.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Client>().WithMany()
            .HasForeignKey(trip => new { trip.CompanyId, trip.ClientId })
            .HasPrincipalKey(client => new { client.CompanyId, client.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Route>().WithMany()
            .HasForeignKey(trip => new { trip.CompanyId, trip.RouteId })
            .HasPrincipalKey(route => new { route.CompanyId, route.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RouteSchedule>().WithMany()
            .HasForeignKey(trip => new { trip.CompanyId, trip.RouteScheduleId })
            .HasPrincipalKey(schedule => new { schedule.CompanyId, schedule.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany()
            .HasForeignKey(trip => new { trip.CompanyId, trip.SubmittedByEmployeeId })
            .HasPrincipalKey(employee => new { employee.CompanyId, employee.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(trip => trip.Assignments).WithOne()
            .HasForeignKey(assignment => new { assignment.CompanyId, assignment.TripId })
            .HasPrincipalKey(trip => new { trip.CompanyId, trip.Id })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(trip => trip.StatusHistory).WithOne()
            .HasForeignKey(history => new { history.CompanyId, history.TripId })
            .HasPrincipalKey(trip => new { trip.CompanyId, trip.Id })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(trip => trip.Reviews).WithOne()
            .HasForeignKey(review => new { review.CompanyId, review.TripId })
            .HasPrincipalKey(trip => new { trip.CompanyId, trip.Id })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(trip => trip.Incidents).WithOne()
            .HasForeignKey(incident => new { incident.CompanyId, incident.TripId })
            .HasPrincipalKey(trip => new { trip.CompanyId, trip.Id })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(trip => trip.Files).WithOne()
            .HasForeignKey(file => new { file.CompanyId, file.TripId })
            .HasPrincipalKey(trip => new { trip.CompanyId, trip.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(trip => trip.Assignments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(trip => trip.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(trip => trip.Reviews).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(trip => trip.Incidents).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(trip => trip.Files).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(trip => trip.CurrentAssignment);
        builder.Ignore(trip => trip.DomainEvents);
    }

    private static void ConfigureLocation(ComplexPropertyBuilder<RouteLocation> location, string prefix)
    {
        location.IsRequired();
        location.Property(value => value.Address).HasColumnName($"{prefix}_address").HasMaxLength(RouteLocation.AddressMaxLength).IsRequired();
        location.Property(value => value.Latitude).HasColumnName($"{prefix}_latitude").HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);
        location.Property(value => value.Longitude).HasColumnName($"{prefix}_longitude").HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);
    }
}
