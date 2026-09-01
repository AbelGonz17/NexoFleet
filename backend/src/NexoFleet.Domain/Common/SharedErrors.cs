namespace NexoFleet.Domain.Common;

public static class SharedErrors
{
    public static readonly Error EmailRequired = Error.Validation(
        "Email.Required",
        "El correo electrónico es obligatorio.");

    public static readonly Error EmailInvalid = Error.Validation(
        "Email.Invalid",
        "El correo electrónico no es válido.");

    public static readonly Error EmailTooLong = Error.Validation(
        "Email.TooLong",
        $"El correo electrónico no puede superar {Email.MaxLength} caracteres.");

    public static readonly Error PhoneRequired = Error.Validation(
        "Phone.Required",
        "El teléfono es obligatorio.");

    public static readonly Error PhoneTooLong = Error.Validation(
        "Phone.TooLong",
        $"El teléfono no puede superar {PhoneNumber.MaxLength} caracteres.");
}
