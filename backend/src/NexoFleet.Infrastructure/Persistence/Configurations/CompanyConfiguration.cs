using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(company => company.Id);

        builder.Property(company => company.Name)
            .HasMaxLength(Company.NameMaxLength)
            .IsRequired();

        builder.Property(company => company.TaxIdentification)
            .HasMaxLength(Company.TaxIdentificationMaxLength)
            .IsRequired();

        builder.HasIndex(company => company.TaxIdentification)
            .IsUnique();

        builder.Property(company => company.Country)
            .HasMaxLength(Company.CountryMaxLength)
            .IsRequired();

        builder.Property(company => company.City)
            .HasMaxLength(Company.CityMaxLength)
            .IsRequired();

        builder.Property(company => company.Phone)
            .HasMaxLength(Company.PhoneMaxLength)
            .IsRequired();

        builder.Property(company => company.Email)
            .HasMaxLength(Company.EmailMaxLength)
            .IsRequired();

        builder.Property(company => company.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(company => company.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(company => company.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Ignore(company => company.DomainEvents);
    }
}
