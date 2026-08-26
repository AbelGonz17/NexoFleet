using NexoFleet.Application.Abstractions.Time;

namespace NexoFleet.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

