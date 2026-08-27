using Microsoft.AspNetCore.Identity;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Guid? CompanyId { get; set; }

    public Company? Company { get; set; }

    public bool IsActive { get; set; } = true;
}
