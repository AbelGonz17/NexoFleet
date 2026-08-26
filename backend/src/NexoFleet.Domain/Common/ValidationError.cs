namespace NexoFleet.Domain.Common;

public sealed record ValidationError(
    IReadOnlyDictionary<string, string[]> Errors)
    : Error(
        "Validation.Error",
        "Uno o más datos enviados no son válidos.",
        ErrorType.Validation);
