namespace NexoFleet.Application.Abstractions.Context;

public interface ICurrentTenant
{
    Guid? CompanyId { get; }

    bool IsAvailable { get; }
}

