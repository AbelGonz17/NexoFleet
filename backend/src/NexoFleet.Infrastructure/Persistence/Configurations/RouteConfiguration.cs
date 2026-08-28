using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("routes");

        builder.HasKey(route => route.Id);

        builder.Property(route => route.RouteCode)
            .HasMaxLength(Route.RouteCodeMaxLength)
            .IsRequired();

        builder.Property(route => route.Name)
            .HasMaxLength(Route.NameMaxLength)
            .IsRequired();

        var origin = builder.ComplexProperty(route => route.Origin);
        origin.IsRequired();
        origin.Property(location => location.Address)
            .HasColumnName("origin_address")
            .HasMaxLength(RouteLocation.AddressMaxLength)
            .IsRequired();
        origin.Property(location => location.Latitude)
            .HasColumnName("origin_latitude")
            .HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);
        origin.Property(location => location.Longitude)
            .HasColumnName("origin_longitude")
            .HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);

        var destination = builder.ComplexProperty(route => route.Destination);
        destination.IsRequired();
        destination.Property(location => location.Address)
            .HasColumnName("destination_address")
            .HasMaxLength(RouteLocation.AddressMaxLength)
            .IsRequired();
        destination.Property(location => location.Latitude)
            .HasColumnName("destination_latitude")
            .HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);
        destination.Property(location => location.Longitude)
            .HasColumnName("destination_longitude")
            .HasPrecision(RouteLocation.CoordinatePrecision, RouteLocation.CoordinateScale);

        builder.Property(route => route.Instructions)
            .HasMaxLength(Route.InstructionsMaxLength);

        builder.Property(route => route.ReferenceAmount)
            .HasPrecision(18, 2);

        builder.Property(route => route.ReferenceCurrency)
            .HasMaxLength(Route.CurrencyLength);

        builder.Property(route => route.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(route => route.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(route => route.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(route => new { route.CompanyId, route.RouteCode })
            .IsUnique();

        builder.HasIndex(route => route.ClientId);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(route => route.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(route => route.Stops)
            .WithOne()
            .HasForeignKey(stop => stop.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(route => route.Stops)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(route => route.DomainEvents);
    }
}
