namespace NexoFleet.Application.Trips.Dtos;

public sealed record SubmitUnexpectedTripRequest(
    string TripNumber,
    Guid SubmittedByEmployeeId,
    DateOnly ServiceDate,
    TripLocationDto Origin,
    TripLocationDto Destination,
    Guid? ClientId = null,
    Guid? RouteId = null,
    decimal? ProposedAmount = null,
    string? Currency = null);
