using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(vehicle => vehicle.Id);

        builder.Property(vehicle => vehicle.LicensePlate)
            .HasMaxLength(Vehicle.LicensePlateMaxLength)
            .IsRequired();

        builder.Property(vehicle => vehicle.Make)
            .HasMaxLength(Vehicle.MakeMaxLength)
            .IsRequired();

        builder.Property(vehicle => vehicle.Model)
            .HasMaxLength(Vehicle.ModelMaxLength)
            .IsRequired();

        builder.Property(vehicle => vehicle.Color)
            .HasMaxLength(Vehicle.ColorMaxLength);

        builder.Property(vehicle => vehicle.OwnershipType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(vehicle => vehicle.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(vehicle => vehicle.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(vehicle => vehicle.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(vehicle => vehicle.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(vehicle => new { vehicle.CompanyId, vehicle.LicensePlate })
            .IsUnique();

        builder.HasIndex(vehicle => vehicle.OwnerEmployeeId);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(vehicle => vehicle.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(vehicle => new { vehicle.CompanyId, vehicle.OwnerEmployeeId })
            .HasPrincipalKey(employee => new { employee.CompanyId, employee.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(vehicle => vehicle.DomainEvents);
    }
}
