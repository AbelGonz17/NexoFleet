using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(client => client.Id);
        builder.HasAlternateKey(client => new { client.CompanyId, client.Id });

        builder.Property(client => client.ClientCode).HasMaxLength(Client.ClientCodeMaxLength).IsRequired();
        builder.Property(client => client.Name).HasMaxLength(Client.NameMaxLength).IsRequired();
        builder.Property(client => client.TaxIdentification).HasMaxLength(Client.TaxIdentificationMaxLength);
        builder.Property(client => client.ContactName).HasMaxLength(Client.ContactNameMaxLength);
        builder.Property(client => client.Phone).HasMaxLength(Client.PhoneMaxLength);
        builder.Property(client => client.Email).HasMaxLength(Client.EmailMaxLength);
        builder.Property(client => client.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(client => client.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(client => client.UpdatedAtUtc).HasColumnType("timestamp with time zone");

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
