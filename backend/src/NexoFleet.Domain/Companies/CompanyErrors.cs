using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Companies;

public static class CompanyErrors
{
    public static readonly Error InvalidId = Error.Validation(
        "Company.InvalidId",
        "El identificador de la empresa no es válido.");

    public static readonly Error NameRequired = Error.Validation(
        "Company.NameRequired",
        "El nombre de la empresa es obligatorio.");

    public static readonly Error NameTooLong = Error.Validation(
        "Company.NameTooLong",
        $"El nombre no puede superar {CompanyName.MaxLength} caracteres.");

    public static readonly Error TaxIdentificationRequired = Error.Validation(
        "Company.TaxIdentificationRequired",
        "La identificación fiscal es obligatoria.");

    public static readonly Error TaxIdentificationTooLong = Error.Validation(
        "Company.TaxIdentificationTooLong",
        $"La identificación fiscal no puede superar {TaxIdentification.MaxLength} caracteres.");

    public static readonly Error CountryRequired = Error.Validation(
        "Company.CountryRequired",
        "El país es obligatorio.");

    public static readonly Error CountryTooLong = Error.Validation(
        "Company.CountryTooLong",
        $"El país no puede superar {Address.CountryMaxLength} caracteres.");

    public static readonly Error CityRequired = Error.Validation(
        "Company.CityRequired",
        "La ciudad es obligatoria.");

    public static readonly Error CityTooLong = Error.Validation(
        "Company.CityTooLong",
        $"La ciudad no puede superar {Address.CityMaxLength} caracteres.");

    public static readonly Error PhoneRequired = Error.Validation(
        "Company.PhoneRequired",
        "El teléfono es obligatorio.");

    public static readonly Error PhoneTooLong = Error.Validation(
        "Company.PhoneTooLong",
        $"El teléfono no puede superar {PhoneNumber.MaxLength} caracteres.");

    public static readonly Error EmailInvalid = Error.Validation(
        "Company.EmailInvalid",
        "El correo electrónico no es válido.");

    public static readonly Error EmailTooLong = Error.Validation(
        "Company.EmailTooLong",
        $"El correo electrónico no puede superar {Email.MaxLength} caracteres.");

    public static readonly Error AlreadyActive = Error.Conflict(
        "Company.AlreadyActive",
        "La empresa ya está activa.");

    public static readonly Error AlreadySuspended = Error.Conflict(
        "Company.AlreadySuspended",
        "La empresa ya está suspendida.");
}

