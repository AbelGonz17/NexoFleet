namespace NexoFleet.Domain.RouteSchedules;

public sealed class RouteScheduleDay
{
    internal RouteScheduleDay(Guid routeScheduleId, DayOfWeek dayOfWeek)
    {
        RouteScheduleId = routeScheduleId;
        DayOfWeek = dayOfWeek;
    }

    private RouteScheduleDay()
    {
    }

    public Guid RouteScheduleId { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }
}
