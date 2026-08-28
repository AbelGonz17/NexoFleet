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

        builder.Property(route => route.Origin)
            .HasMaxLength(Route.OriginMaxLength)
            .IsRequired();

        builder.Property(route => route.Destination)
            .HasMaxLength(Route.DestinationMaxLength)
            .IsRequired();

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
