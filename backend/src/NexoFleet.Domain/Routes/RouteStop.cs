using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Routes;

public sealed class RouteStop : Entity
{
    public const int AddressMaxLength = 300;
    public const int InstructionsMaxLength = 500;

    internal RouteStop(
        Guid id,
        Guid routeId,
        int sequence,
        string address,
        string? instructions) : base(id)
    {
        RouteId = routeId;
        Sequence = sequence;
        Address = address;
        Instructions = instructions;
    }

    private RouteStop()
    {
    }

    public Guid RouteId { get; private set; }

    public int Sequence { get; private set; }

    public string Address { get; private set; } = string.Empty;

    public string? Instructions { get; private set; }

    internal void Update(string address, string? instructions)
    {
        Address = address;
        Instructions = instructions;
    }

    internal void ChangeSequence(int sequence) => Sequence = sequence;
}
