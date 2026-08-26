namespace NexoFleet.Application.Authorization;

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Administrator = "Administrator";
    public const string Employee = "Employee";

    public static readonly string[] All = [SuperAdmin, Administrator, Employee];
}
