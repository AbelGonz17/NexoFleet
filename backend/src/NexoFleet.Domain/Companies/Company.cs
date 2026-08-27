using System.Net.Mail;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies.Events;

namespace NexoFleet.Domain.Companies;

public sealed class Company : AggregateRoot
{
    public const int NameMaxLength = 200;
    public const int TaxIdentificationMaxLength = 50;
    public const int CountryMaxLength = 100;
    public const int CityMaxLength = 100;
    public const int PhoneMaxLength = 30;
    public const int EmailMaxLength = 256;

    private Company(
        Guid id,
        string name,
        string taxIdentification,
        string country,
        string city,
        string phone,
        string email,
        DateTimeOffset createdAtUtc) : base(id)
    {
        Name = name;
        TaxIdentification = taxIdentification;
        Country = country;
        City = city;
        Phone = phone;
        Email = email;
        Status = CompanyStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Company()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string TaxIdentification { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public CompanyStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Company> Create(
        Guid id,
        string name,
        string taxIdentification,
        string country,
        string city,
        string phone,
        string email,
        DateTimeOffset createdAtUtc)
    {
        var validationResult = ValidateProfile(
            id,
            name,
            taxIdentification,
            country,
            city,
            phone,
            email);

        if (validationResult.IsFailure)
        {
            return Result<Company>.Failure(validationResult.Error);
        }

        var company = new Company(
            id,
            Normalize(name),
            NormalizeTaxIdentification(taxIdentification),
            Normalize(country),
            Normalize(city),
            Normalize(phone),
            NormalizeEmail(email),
            createdAtUtc);

        company.RaiseDomainEvent(new CompanyCreatedDomainEvent(company.Id, createdAtUtc));
        return Result<Company>.Success(company);
    }

    public Result UpdateProfile(
        string name,
        string taxIdentification,
        string country,
        string city,
        string phone,
        string email,
        DateTimeOffset updatedAtUtc)
    {
        var validationResult = ValidateProfile(
            Id,
            name,
            taxIdentification,
            country,
            city,
            phone,
            email);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var normalizedName = Normalize(name);
        var normalizedTaxIdentification = NormalizeTaxIdentification(taxIdentification);
        var normalizedCountry = Normalize(country);
        var normalizedCity = Normalize(city);
        var normalizedPhone = Normalize(phone);
        var normalizedEmail = NormalizeEmail(email);

        if (Name == normalizedName &&
            TaxIdentification == normalizedTaxIdentification &&
            Country == normalizedCountry &&
            City == normalizedCity &&
            Phone == normalizedPhone &&
            Email == normalizedEmail)
        {
            return Result.Success();
        }

        Name = normalizedName;
        TaxIdentification = normalizedTaxIdentification;
        Country = normalizedCountry;
        City = normalizedCity;
        Phone = normalizedPhone;
        Email = normalizedEmail;
        UpdatedAtUtc = updatedAtUtc;

        RaiseDomainEvent(new CompanyProfileUpdatedDomainEvent(Id, updatedAtUtc));
        return Result.Success();
    }

    public Result Suspend(DateTimeOffset occurredAt)
    {
        if (Status == CompanyStatus.Suspended)
        {
            return Result.Failure(CompanyErrors.AlreadySuspended);
        }

        ChangeStatus(CompanyStatus.Suspended, occurredAt);
        return Result.Success();
    }

    public Result Activate(DateTimeOffset occurredAt)
    {
        if (Status == CompanyStatus.Active)
        {
            return Result.Failure(CompanyErrors.AlreadyActive);
        }

        ChangeStatus(CompanyStatus.Active, occurredAt);
        return Result.Success();
    }

    private static Result ValidateProfile(
        Guid id,
        string name,
        string taxIdentification,
        string country,
        string city,
        string phone,
        string email)
    {
        if (id == Guid.Empty) return Result.Failure(CompanyErrors.InvalidId);
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure(CompanyErrors.NameRequired);
        if (name.Trim().Length > NameMaxLength) return Result.Failure(CompanyErrors.NameTooLong);
        if (string.IsNullOrWhiteSpace(taxIdentification)) return Result.Failure(CompanyErrors.TaxIdentificationRequired);
        if (taxIdentification.Trim().Length > TaxIdentificationMaxLength) return Result.Failure(CompanyErrors.TaxIdentificationTooLong);
        if (string.IsNullOrWhiteSpace(country)) return Result.Failure(CompanyErrors.CountryRequired);
        if (country.Trim().Length > CountryMaxLength) return Result.Failure(CompanyErrors.CountryTooLong);
        if (string.IsNullOrWhiteSpace(city)) return Result.Failure(CompanyErrors.CityRequired);
        if (city.Trim().Length > CityMaxLength) return Result.Failure(CompanyErrors.CityTooLong);
        if (string.IsNullOrWhiteSpace(phone)) return Result.Failure(CompanyErrors.PhoneRequired);
        if (phone.Trim().Length > PhoneMaxLength) return Result.Failure(CompanyErrors.PhoneTooLong);
        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email.Trim(), out _)) return Result.Failure(CompanyErrors.EmailInvalid);
        if (email.Trim().Length > EmailMaxLength) return Result.Failure(CompanyErrors.EmailTooLong);

        return Result.Success();
    }

    private void ChangeStatus(CompanyStatus newStatus, DateTimeOffset occurredAt)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAt;
        RaiseDomainEvent(new CompanyStatusChangedDomainEvent(
            Id,
            previousStatus,
            newStatus,
            occurredAt));
    }

    private static string Normalize(string value) => value.Trim();

    private static string NormalizeTaxIdentification(string value) =>
        value.Trim().ToUpperInvariant();

    private static string NormalizeEmail(string value) =>
        value.Trim().ToLowerInvariant();
}
