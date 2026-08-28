using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Notifications;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(notification => notification.Title).HasMaxLength(NotificationErrors.TitleMaxLength).IsRequired();
        builder.Property(notification => notification.Message).HasMaxLength(NotificationErrors.MessageMaxLength).IsRequired();
        builder.Property(notification => notification.RelatedEntityType).HasMaxLength(NotificationErrors.RelatedEntityTypeMaxLength);
        builder.Property(notification => notification.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(notification => notification.ReadAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(notification => notification.ArchivedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(notification => notification.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(notification => new { notification.RecipientUserId, notification.Status, notification.CreatedAtUtc });
        builder.HasIndex(notification => new { notification.CompanyId, notification.RelatedEntityType, notification.RelatedEntityId });

        builder.HasOne<Company>().WithMany().HasForeignKey(notification => notification.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(notification => notification.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Employee>().WithMany()
            .HasForeignKey(notification => new { notification.CompanyId, notification.RecipientEmployeeId })
            .HasPrincipalKey(employee => new { employee.CompanyId, employee.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(notification => notification.DomainEvents);
    }
}
