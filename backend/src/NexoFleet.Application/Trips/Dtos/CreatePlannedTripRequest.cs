namespace NexoFleet.Application.Trips.Dtos;

public sealed record CreatePlannedTripRequest(
    string TripNumber,
    DateOnly ServiceDate,
    TripLocationDto Origin,
    TripLocationDto Destination,
    Guid? ClientId = null,
    Guid? RouteId = null,
    Guid? RouteScheduleId = null,
    decimal? AgreedAmount = null,
    string? Currency = null);
