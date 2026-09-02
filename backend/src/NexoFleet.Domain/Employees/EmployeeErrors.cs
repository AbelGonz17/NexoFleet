using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Employees;

public static class EmployeeErrors
{
    public static readonly Error InvalidId = Error.Validation(
        "Employee.InvalidId",
        "El identificador del empleado no es válido.");

    public static readonly Error InvalidCompanyId = Error.Validation(
        "Employee.InvalidCompanyId",
        "El identificador de la empresa no es válido.");

    public static readonly Error EmployeeCodeRequired = Error.Validation(
        "Employee.EmployeeCodeRequired",
        "El código del empleado es obligatorio.");

    public static readonly Error EmployeeCodeTooLong = Error.Validation(
        "Employee.EmployeeCodeTooLong",
        $"El código del empleado no puede superar {EmployeeCode.MaxLength} caracteres.");

    public static readonly Error FirstNameRequired = Error.Validation(
        "Employee.FirstNameRequired",
        "El nombre del empleado es obligatorio.");

    public static readonly Error FirstNameTooLong = Error.Validation(
        "Employee.FirstNameTooLong",
        $"El nombre no puede superar {FullName.FirstNameMaxLength} caracteres.");

    public static readonly Error LastNameRequired = Error.Validation(
        "Employee.LastNameRequired",
        "El apellido del empleado es obligatorio.");

    public static readonly Error LastNameTooLong = Error.Validation(
        "Employee.LastNameTooLong",
        $"El apellido no puede superar {FullName.LastNameMaxLength} caracteres.");

    public static readonly Error IdentityDocumentRequired = Error.Validation(
        "Employee.IdentityDocumentRequired",
        "El documento de identidad es obligatorio.");

    public static readonly Error IdentityDocumentTooLong = Error.Validation(
        "Employee.IdentityDocumentTooLong",
        $"El documento de identidad no puede superar {IdentityDocument.MaxLength} caracteres.");

    public static readonly Error PhoneRequired = Error.Validation(
        "Employee.PhoneRequired",
        "El teléfono del empleado es obligatorio.");

    public static readonly Error PhoneTooLong = Error.Validation(
        "Employee.PhoneTooLong",
        $"El teléfono no puede superar {PhoneNumber.MaxLength} caracteres.");

    public static readonly Error EmailInvalid = Error.Validation(
        "Employee.EmailInvalid",
        "El correo electrónico del empleado no es válido.");

    public static readonly Error EmailTooLong = Error.Validation(
        "Employee.EmailTooLong",
        $"El correo electrónico no puede superar {Email.MaxLength} caracteres.");

    public static readonly Error HireDateInFuture = Error.Validation(
        "Employee.HireDateInFuture",
        "La fecha de contratación no puede estar en el futuro.");

    public static readonly Error AlreadyActive = Error.Conflict(
        "Employee.AlreadyActive",
        "El empleado ya está activo.");

    public static readonly Error AlreadySuspended = Error.Conflict(
        "Employee.AlreadySuspended",
        "El empleado ya está suspendido.");

    public static readonly Error AlreadyRetired = Error.Conflict(
        "Employee.AlreadyRetired",
        "El empleado ya está retirado.");

    public static readonly Error RetiredStatusIsFinal = Error.Conflict(
        "Employee.RetiredStatusIsFinal",
        "Un empleado retirado no puede cambiar nuevamente de estado.");

    public static readonly Error InvalidUserId = Error.Validation(
        "Employee.InvalidUserId",
        "El identificador del usuario no es válido.");

    public static readonly Error UserAccountAlreadyLinked = Error.Conflict(
        "Employee.UserAccountAlreadyLinked",
        "El empleado ya tiene una cuenta de usuario vinculada.");

    public static readonly Error UserAccountNotLinked = Error.Conflict(
        "Employee.UserAccountNotLinked",
        "El empleado no tiene una cuenta de usuario vinculada.");

    public static readonly Error NotFound = Error.NotFound(
        "Employee.NotFound",
        "El empleado no fue encontrado.");

    public static readonly Error EmployeeCodeDuplicate = Error.Conflict(
        "Employee.EmployeeCodeDuplicate",
        "Ya existe un empleado con el mismo código en la empresa.");

    public static readonly Error IdentityDocumentDuplicate = Error.Conflict(
        "Employee.IdentityDocumentDuplicate",
        "Ya existe un empleado con el mismo documento de identidad en la empresa.");

    public static readonly Error EmailDuplicate = Error.Conflict(
        "Employee.EmailDuplicate",
        "Ya existe un empleado con el mismo correo electrónico en la empresa.");
}
