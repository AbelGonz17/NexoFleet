using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Common;
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

        builder.HasAlternateKey(employee => new { employee.CompanyId, employee.Id });

        builder.Property(employee => employee.EmployeeCode)
            .HasConversion(code => code.Value, value => EmployeeCode.Create(value).Value)
            .HasMaxLength(EmployeeCode.MaxLength)
            .IsRequired();

        builder.ComplexProperty(employee => employee.FullName, fullNameBuilder =>
        {
            fullNameBuilder.IsRequired();
            fullNameBuilder.Property(name => name.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(FullName.FirstNameMaxLength)
                .IsRequired();

            fullNameBuilder.Property(name => name.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(FullName.LastNameMaxLength)
                .IsRequired();
        });

        builder.Property(employee => employee.IdentityDocument)
            .HasConversion(doc => doc.Value, value => IdentityDocument.Create(value).Value)
            .HasMaxLength(IdentityDocument.MaxLength)
            .IsRequired();

        builder.Property(employee => employee.Phone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value, null, null).Value)
            .HasMaxLength(PhoneNumber.MaxLength)
            .IsRequired();

        builder.Property(employee => employee.Email)
            .HasConversion(email => email.Value, value => Email.Create(value, null, null, null).Value)
            .HasMaxLength(Email.MaxLength)
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
