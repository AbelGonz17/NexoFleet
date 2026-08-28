using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class PaymentPeriodConfiguration : IEntityTypeConfiguration<PaymentPeriod>
{
    public void Configure(EntityTypeBuilder<PaymentPeriod> builder)
    {
        builder.ToTable("payment_periods", table =>
            table.HasCheckConstraint("CK_payment_periods_dates", "\"EndsOn\" >= \"StartsOn\""));
        builder.HasKey(period => period.Id);
        builder.HasAlternateKey(period => new { period.CompanyId, period.Id });
        builder.Property(period => period.Code).HasMaxLength(PaymentErrors.CodeMaxLength).IsRequired();
        builder.Property(period => period.StartsOn).HasColumnType("date").IsRequired();
        builder.Property(period => period.EndsOn).HasColumnType("date").IsRequired();
        builder.Property(period => period.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(period => period.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(period => period.UpdatedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(period => new { period.CompanyId, period.Code }).IsUnique();
        builder.HasIndex(period => new { period.CompanyId, period.StartsOn, period.EndsOn });
        builder.HasOne<Company>().WithMany().HasForeignKey(period => period.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(period => period.DomainEvents);
    }
}
