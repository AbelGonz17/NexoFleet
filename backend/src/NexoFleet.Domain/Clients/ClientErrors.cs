using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Domain.Clients;

public static class ClientErrors
{
    public static readonly Error InvalidId = Error.Validation("Client.InvalidId", "The client identifier is invalid.");
    public static readonly Error InvalidCompanyId = Error.Validation("Client.InvalidCompanyId", "The company identifier is invalid.");
    public static readonly Error ClientCodeRequired = Error.Validation("Client.ClientCodeRequired", "The client code is required.");
    public static readonly Error ClientCodeTooLong = Error.Validation("Client.ClientCodeTooLong", $"The client code cannot exceed {ClientCode.MaxLength} characters.");
    public static readonly Error NameRequired = Error.Validation("Client.NameRequired", "The client name is required.");
    public static readonly Error NameTooLong = Error.Validation("Client.NameTooLong", $"The client name cannot exceed {ClientName.MaxLength} characters.");
    public static readonly Error TaxIdentificationTooLong = Error.Validation("Client.TaxIdentificationTooLong", $"The tax identification cannot exceed {TaxIdentification.MaxLength} characters.");
    public static readonly Error ContactNameTooLong = Error.Validation("Client.ContactNameTooLong", $"The contact name cannot exceed {ContactName.MaxLength} characters.");
    public static readonly Error PhoneTooLong = Error.Validation("Client.PhoneTooLong", $"The phone cannot exceed {PhoneNumber.MaxLength} characters.");
    public static readonly Error EmailInvalid = Error.Validation("Client.EmailInvalid", "The email is invalid.");
    public static readonly Error EmailTooLong = Error.Validation("Client.EmailTooLong", $"The email cannot exceed {Email.MaxLength} characters.");
    public static readonly Error AlreadyActive = Error.Conflict("Client.AlreadyActive", "The client is already active.");
    public static readonly Error AlreadyInactive = Error.Conflict("Client.AlreadyInactive", "The client is already inactive.");
}
