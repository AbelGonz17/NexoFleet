namespace NexoFleet.Application.Authentication;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid? CompanyId,
    IReadOnlyCollection<string> Roles);
