namespace NexoFleet.Application.Trips.Dtos;

public sealed record CompleteTripRequest(
    decimal FinalAmount,
    string Currency);
