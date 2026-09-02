namespace NexoFleet.Application.Trips.Dtos;

public sealed record UpdateTripPlanRequest(
    DateOnly ServiceDate,
    TripLocationDto Origin,
    TripLocationDto Destination,
    Guid? ClientId = null,
    Guid? RouteId = null,
    decimal? AgreedAmount = null,
    string? Currency = null);
