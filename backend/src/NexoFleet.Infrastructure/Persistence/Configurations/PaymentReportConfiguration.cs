using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class PaymentReportConfiguration : IEntityTypeConfiguration<PaymentReport>
{
    public void Configure(EntityTypeBuilder<PaymentReport> builder)
    {
        builder.ToTable("payment_reports", table =>
            table.HasCheckConstraint("ck_payment_reports_base_amount", "\"base_amount\" >= 0"));
        builder.HasKey(report => report.Id);
        builder.HasAlternateKey(report => new { report.CompanyId, report.Id });
        builder.Property(report => report.BaseAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(report => report.Currency).HasMaxLength(PaymentErrors.CurrencyLength).IsRequired();
        builder.Property(report => report.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(report => report.PublishedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(report => report.VoidedReason).HasMaxLength(PaymentErrors.ReasonMaxLength);
        builder.Property(report => report.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(report => report.UpdatedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(report => new { report.CompanyId, report.PaymentPeriodId, report.EmployeeId }).IsUnique();
        builder.HasIndex(report => new { report.CompanyId, report.EmployeeId, report.Status });

        builder.HasOne<PaymentPeriod>().WithMany()
            .HasForeignKey(report => new { report.CompanyId, report.PaymentPeriodId })
            .HasPrincipalKey(period => new { period.CompanyId, period.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany()
            .HasForeignKey(report => new { report.CompanyId, report.EmployeeId })
            .HasPrincipalKey(employee => new { employee.CompanyId, employee.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(report => report.Items).WithOne()
            .HasForeignKey(item => new { item.CompanyId, item.PaymentReportId })
            .HasPrincipalKey(report => new { report.CompanyId, report.Id })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(report => report.Comments).WithOne()
            .HasForeignKey(comment => new { comment.CompanyId, comment.PaymentReportId })
            .HasPrincipalKey(report => new { report.CompanyId, report.Id })
            .HasConstraintName("fk_payment_comments_payment_reports")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(report => report.Files).WithOne()
            .HasForeignKey(file => new { file.CompanyId, file.PaymentReportId })
            .HasPrincipalKey(report => new { report.CompanyId, report.Id })
            .HasConstraintName("fk_payment_report_files_payment_reports")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(report => report.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(report => report.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(report => report.Files).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(report => report.Additions);
        builder.Ignore(report => report.Deductions);
        builder.Ignore(report => report.TotalAmount);
        builder.Ignore(report => report.DomainEvents);
    }
}
