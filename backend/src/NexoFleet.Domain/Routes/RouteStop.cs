using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Routes;

public sealed class RouteStop : Entity
{
    public const int InstructionsMaxLength = 500;

    internal RouteStop(
        Guid id,
        Guid routeId,
        int sequence,
        RouteLocation location,
        string? instructions) : base(id)
    {
        RouteId = routeId;
        Sequence = sequence;
        Location = location;
        Instructions = instructions;
    }

    private RouteStop()
    {
    }

    public Guid RouteId { get; private set; }

    public int Sequence { get; private set; }

    public RouteLocation Location { get; private set; } = null!;

    public string? Instructions { get; private set; }

    internal void Update(RouteLocation location, string? instructions)
    {
        Location = location;
        Instructions = instructions;
    }

    internal void ChangeSequence(int sequence) => Sequence = sequence;
}
