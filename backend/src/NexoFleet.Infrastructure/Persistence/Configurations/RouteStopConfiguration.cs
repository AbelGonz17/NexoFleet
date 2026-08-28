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

        builder.Property(stop => stop.Address)
            .HasMaxLength(RouteStop.AddressMaxLength)
            .IsRequired();

        builder.Property(stop => stop.Instructions)
            .HasMaxLength(RouteStop.InstructionsMaxLength);

        builder.HasIndex(stop => new { stop.RouteId, stop.Sequence })
            .IsUnique();

        builder.Ignore(stop => stop.DomainEvents);
    }
}
