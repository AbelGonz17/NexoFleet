using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class RouteStopConfiguration : IEntityTypeConfiguration<RouteStop>
{
    public void Configure(EntityTypeBuilder<RouteStop> builder)
    {
        builder.ToTable("route_stops");

        builder.HasKey(stop => stop.Id);

        var location = builder.ComplexProperty(stop => stop.Location);
        location.IsRequired();
        location.Property(value => value.Address)
            .HasColumnName("address")
            .HasMaxLength(RouteLocation.AddressMaxLength)
            .IsRequired();
        location.Property(value => value.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);
        location.Property(value => value.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);

        builder.Property(stop => stop.Instructions)
            .HasMaxLength(RouteStop.InstructionsMaxLength);

        builder.HasIndex(stop => new { stop.RouteId, stop.Sequence })
            .IsUnique();

        builder.Ignore(stop => stop.DomainEvents);
    }
}
