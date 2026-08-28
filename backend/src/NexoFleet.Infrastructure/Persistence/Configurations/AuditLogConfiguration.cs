using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Auditing;
using NexoFleet.Domain.Companies;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Action).HasMaxLength(AuditLogErrors.ActionMaxLength).IsRequired();
        builder.Property(log => log.EntityType).HasMaxLength(AuditLogErrors.EntityTypeMaxLength).IsRequired();
        builder.Property(log => log.Data).HasColumnType("jsonb").HasMaxLength(AuditLogErrors.DataMaxLength);
        builder.Property(log => log.IpAddress).HasMaxLength(AuditLogErrors.IpAddressMaxLength);
        builder.Property(log => log.UserAgent).HasMaxLength(AuditLogErrors.UserAgentMaxLength);
        builder.Property(log => log.OccurredAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(log => new { log.CompanyId, log.OccurredAtUtc });
        builder.HasIndex(log => new { log.EntityType, log.EntityId, log.OccurredAtUtc });
        builder.HasIndex(log => new { log.ActorUserId, log.OccurredAtUtc });
        builder.HasOne<Company>().WithMany().HasForeignKey(log => log.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(log => log.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(log => log.DomainEvents);
    }
}
