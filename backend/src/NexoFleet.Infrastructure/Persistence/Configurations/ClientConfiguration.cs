using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(client => client.Id);
        builder.HasAlternateKey(client => new { client.CompanyId, client.Id });

        builder.Property(client => client.ClientCode)
            .HasConversion(code => code.Value, value => ClientCode.Create(value).Value)
            .HasMaxLength(ClientCode.MaxLength)
            .IsRequired();

        builder.Property(client => client.Name)
            .HasConversion(name => name.Value, value => ClientName.Create(value).Value)
            .HasMaxLength(ClientName.MaxLength)
            .IsRequired();

        builder.Property(client => client.TaxIdentification)
            .HasConversion(taxId => taxId != null ? taxId.Value : null, value => value != null ? TaxIdentification.Create(value).Value : null)
            .HasMaxLength(TaxIdentification.MaxLength);

        builder.Property(client => client.ContactName)
            .HasConversion(contact => contact != null ? contact.Value : null, value => value != null ? ContactName.Create(value).Value : null)
            .HasMaxLength(ContactName.MaxLength);

        builder.Property(client => client.Phone)
            .HasConversion(phone => phone != null ? phone.Value : null, value => value != null ? PhoneNumber.Create(value, null, null).Value : null)
            .HasMaxLength(PhoneNumber.MaxLength);

        builder.Property(client => client.Email)
            .HasConversion(email => email != null ? email.Value : null, value => value != null ? Email.Create(value, null, null, null).Value : null)
            .HasMaxLength(Email.MaxLength);

        builder.Property(client => client.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(client => client.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(client => client.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(client => new { client.CompanyId, client.ClientCode }).IsUnique();
        builder.HasIndex(client => new { client.CompanyId, client.TaxIdentification })
            .IsUnique()
            .HasFilter("\"tax_identification\" IS NOT NULL");

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(client => client.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(client => client.DomainEvents);
    }
}
