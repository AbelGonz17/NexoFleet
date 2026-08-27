using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.EmployeeCode)
            .HasMaxLength(Employee.EmployeeCodeMaxLength)
            .IsRequired();

        builder.Property(employee => employee.FirstName)
            .HasMaxLength(Employee.FirstNameMaxLength)
            .IsRequired();

        builder.Property(employee => employee.LastName)
            .HasMaxLength(Employee.LastNameMaxLength)
            .IsRequired();

        builder.Property(employee => employee.IdentityDocument)
            .HasMaxLength(Employee.IdentityDocumentMaxLength)
            .IsRequired();

        builder.Property(employee => employee.Phone)
            .HasMaxLength(Employee.PhoneMaxLength)
            .IsRequired();

        builder.Property(employee => employee.Email)
            .HasMaxLength(Employee.EmailMaxLength)
            .IsRequired();

        builder.Property(employee => employee.HireDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(employee => employee.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(employee => employee.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(employee => employee.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(employee => new { employee.CompanyId, employee.EmployeeCode })
            .IsUnique();

        builder.HasIndex(employee => new { employee.CompanyId, employee.IdentityDocument })
            .IsUnique();

        builder.HasIndex(employee => new { employee.CompanyId, employee.Email })
            .IsUnique();

        builder.HasIndex(employee => employee.UserId)
            .IsUnique();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(employee => employee.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Employee>(employee => employee.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(employee => employee.DomainEvents);
    }
}
