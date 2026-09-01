using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(company => company.Id);

        builder.Property(company => company.Name)
            .HasConversion(name => name.Value, value => CompanyName.Create(value).Value)
            .HasMaxLength(CompanyName.MaxLength)
            .IsRequired();

        builder.Property(company => company.TaxIdentification)
            .HasConversion(taxId => taxId.Value, value => TaxIdentification.Create(value).Value)
            .HasMaxLength(TaxIdentification.MaxLength)
            .IsRequired();

        builder.HasIndex(company => company.TaxIdentification)
            .IsUnique();

        builder.ComplexProperty(company => company.Address, addressBuilder =>
        {
            addressBuilder.IsRequired();
            addressBuilder.Property(address => address.Country)
                .HasColumnName("country")
                .HasMaxLength(Address.CountryMaxLength)
                .IsRequired();

            addressBuilder.Property(address => address.City)
                .HasColumnName("city")
                .HasMaxLength(Address.CityMaxLength)
                .IsRequired();
        });

        builder.Property(company => company.Phone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value, null, null).Value)
            .HasMaxLength(PhoneNumber.MaxLength)
            .IsRequired();

        builder.Property(company => company.Email)
            .HasConversion(email => email.Value, value => Email.Create(value, null, null, null).Value)
            .HasMaxLength(Email.MaxLength)
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
