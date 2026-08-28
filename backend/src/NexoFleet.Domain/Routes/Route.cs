using NexoFleet.Domain.Common;
using NexoFleet.Domain.Routes.Events;

namespace NexoFleet.Domain.Routes;

public sealed class Route : AggregateRoot
{
    public const int RouteCodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int OriginMaxLength = 300;
    public const int DestinationMaxLength = 300;
    public const int InstructionsMaxLength = 1000;
    public const int CurrencyLength = 3;

    private readonly List<RouteStop> _stops = [];

    private Route(
        Guid id,
        Guid companyId,
        Guid? clientId,
        string routeCode,
        string name,
        string origin,
        string destination,
        string? instructions,
        int? estimatedDurationMinutes,
        decimal? referenceAmount,
        string? referenceCurrency,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        ClientId = clientId;
        RouteCode = routeCode;
        Name = name;
        Origin = origin;
        Destination = destination;
        Instructions = instructions;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        ReferenceAmount = referenceAmount;
        ReferenceCurrency = referenceCurrency;
        Status = RouteStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Route()
    {
    }

    public Guid CompanyId { get; private set; }

    public Guid? ClientId { get; private set; }

    public string RouteCode { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Origin { get; private set; } = string.Empty;

    public string Destination { get; private set; } = string.Empty;

    public string? Instructions { get; private set; }

    public int? EstimatedDurationMinutes { get; private set; }

    public decimal? ReferenceAmount { get; private set; }

    public string? ReferenceCurrency { get; private set; }

    public RouteStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<RouteStop> Stops => _stops.AsReadOnly();

    public static Result<Route> Create(
        Guid id,
        Guid companyId,
        Guid? clientId,
        string routeCode,
        string name,
        string origin,
        string destination,
        string? instructions,
        int? estimatedDurationMinutes,
        decimal? referenceAmount,
        string? referenceCurrency,
        DateTimeOffset createdAtUtc)
    {
        var validationResult = ValidateDetails(
            id,
            companyId,
            clientId,
            routeCode,
            name,
            origin,
            destination,
            instructions,
            estimatedDurationMinutes,
            referenceAmount,
            referenceCurrency);

        if (validationResult.IsFailure)
        {
            return Result<Route>.Failure(validationResult.Error);
        }

        var route = new Route(
            id,
            companyId,
            clientId,
            NormalizeIdentifier(routeCode),
            Normalize(name),
            Normalize(origin),
            Normalize(destination),
            NormalizeOptional(instructions),
            estimatedDurationMinutes,
            referenceAmount,
            NormalizeCurrency(referenceCurrency),
            createdAtUtc);

        route.RaiseDomainEvent(new RouteCreatedDomainEvent(
            route.Id,
            route.CompanyId,
            createdAtUtc));

        return Result<Route>.Success(route);
    }

    public Result UpdateDetails(
        Guid? clientId,
        string routeCode,
        string name,
        string origin,
        string destination,
        string? instructions,
        int? estimatedDurationMinutes,
        decimal? referenceAmount,
        string? referenceCurrency,
        DateTimeOffset updatedAtUtc)
    {
        var validationResult = ValidateDetails(
            Id,
            CompanyId,
            clientId,
            routeCode,
            name,
            origin,
            destination,
            instructions,
            estimatedDurationMinutes,
            referenceAmount,
            referenceCurrency);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var normalizedRouteCode = NormalizeIdentifier(routeCode);
        var normalizedName = Normalize(name);
        var normalizedOrigin = Normalize(origin);
        var normalizedDestination = Normalize(destination);
        var normalizedInstructions = NormalizeOptional(instructions);
        var normalizedCurrency = NormalizeCurrency(referenceCurrency);

        if (ClientId == clientId &&
            RouteCode == normalizedRouteCode &&
            Name == normalizedName &&
            Origin == normalizedOrigin &&
            Destination == normalizedDestination &&
            Instructions == normalizedInstructions &&
            EstimatedDurationMinutes == estimatedDurationMinutes &&
            ReferenceAmount == referenceAmount &&
            ReferenceCurrency == normalizedCurrency)
        {
            return Result.Success();
        }

        ClientId = clientId;
        RouteCode = normalizedRouteCode;
        Name = normalizedName;
        Origin = normalizedOrigin;
        Destination = normalizedDestination;
        Instructions = normalizedInstructions;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        ReferenceAmount = referenceAmount;
        ReferenceCurrency = normalizedCurrency;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result AddStop(
        Guid stopId,
        string address,
        string? instructions,
        DateTimeOffset updatedAtUtc)
    {
        var validationResult = ValidateStop(stopId, address, instructions);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (_stops.Any(stop => stop.Id == stopId))
        {
            return Result.Failure(RouteErrors.StopAlreadyExists);
        }

        _stops.Add(new RouteStop(
            stopId,
            Id,
            _stops.Count + 1,
            Normalize(address),
            NormalizeOptional(instructions)));
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result UpdateStop(
        Guid stopId,
        string address,
        string? instructions,
        DateTimeOffset updatedAtUtc)
    {
        var validationResult = ValidateStop(stopId, address, instructions);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var stop = _stops.SingleOrDefault(candidate => candidate.Id == stopId);
        if (stop is null)
        {
            return Result.Failure(RouteErrors.StopNotFound);
        }

        var normalizedAddress = Normalize(address);
        var normalizedInstructions = NormalizeOptional(instructions);
        if (stop.Address == normalizedAddress && stop.Instructions == normalizedInstructions)
        {
            return Result.Success();
        }

        stop.Update(normalizedAddress, normalizedInstructions);
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result MoveStop(
        Guid stopId,
        int newSequence,
        DateTimeOffset updatedAtUtc)
    {
        var currentIndex = _stops.FindIndex(stop => stop.Id == stopId);
        if (currentIndex < 0)
        {
            return Result.Failure(RouteErrors.StopNotFound);
        }

        if (newSequence < 1 || newSequence > _stops.Count)
        {
            return Result.Failure(RouteErrors.InvalidStopSequence);
        }

        var newIndex = newSequence - 1;
        if (currentIndex == newIndex)
        {
            return Result.Success();
        }

        var stop = _stops[currentIndex];
        _stops.RemoveAt(currentIndex);
        _stops.Insert(newIndex, stop);
        ResequenceStops();
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result RemoveStop(Guid stopId, DateTimeOffset updatedAtUtc)
    {
        var stop = _stops.SingleOrDefault(candidate => candidate.Id == stopId);
        if (stop is null)
        {
            return Result.Failure(RouteErrors.StopNotFound);
        }

        _stops.Remove(stop);
        ResequenceStops();
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result Activate(DateTimeOffset occurredAtUtc)
    {
        if (Status == RouteStatus.Active)
        {
            return Result.Failure(RouteErrors.AlreadyActive);
        }

        ChangeStatus(RouteStatus.Active, occurredAtUtc);
        return Result.Success();
    }

    public Result Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (Status == RouteStatus.Inactive)
        {
            return Result.Failure(RouteErrors.AlreadyInactive);
        }

        ChangeStatus(RouteStatus.Inactive, occurredAtUtc);
        return Result.Success();
    }

    private static Result ValidateDetails(
        Guid id,
        Guid companyId,
        Guid? clientId,
        string routeCode,
        string name,
        string origin,
        string destination,
        string? instructions,
        int? estimatedDurationMinutes,
        decimal? referenceAmount,
        string? referenceCurrency)
    {
        if (id == Guid.Empty) return Result.Failure(RouteErrors.InvalidId);
        if (companyId == Guid.Empty) return Result.Failure(RouteErrors.InvalidCompanyId);
        if (clientId == Guid.Empty) return Result.Failure(RouteErrors.InvalidClientId);
        if (string.IsNullOrWhiteSpace(routeCode)) return Result.Failure(RouteErrors.RouteCodeRequired);
        if (routeCode.Trim().Length > RouteCodeMaxLength) return Result.Failure(RouteErrors.RouteCodeTooLong);
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure(RouteErrors.NameRequired);
        if (name.Trim().Length > NameMaxLength) return Result.Failure(RouteErrors.NameTooLong);
        if (string.IsNullOrWhiteSpace(origin)) return Result.Failure(RouteErrors.OriginRequired);
        if (origin.Trim().Length > OriginMaxLength) return Result.Failure(RouteErrors.OriginTooLong);
        if (string.IsNullOrWhiteSpace(destination)) return Result.Failure(RouteErrors.DestinationRequired);
        if (destination.Trim().Length > DestinationMaxLength) return Result.Failure(RouteErrors.DestinationTooLong);
        if (instructions?.Trim().Length > InstructionsMaxLength) return Result.Failure(RouteErrors.InstructionsTooLong);
        if (estimatedDurationMinutes <= 0) return Result.Failure(RouteErrors.InvalidEstimatedDuration);
        if (referenceAmount < 0) return Result.Failure(RouteErrors.InvalidReferenceAmount);
        if (!referenceAmount.HasValue && !string.IsNullOrWhiteSpace(referenceCurrency)) return Result.Failure(RouteErrors.ReferenceAmountRequired);
        if (referenceAmount.HasValue && string.IsNullOrWhiteSpace(referenceCurrency)) return Result.Failure(RouteErrors.ReferenceCurrencyRequired);
        if (!string.IsNullOrWhiteSpace(referenceCurrency) && !IsValidCurrency(referenceCurrency))
        {
            return Result.Failure(RouteErrors.ReferenceCurrencyInvalid);
        }

        return Result.Success();
    }

    private static Result ValidateStop(Guid stopId, string address, string? instructions)
    {
        if (stopId == Guid.Empty) return Result.Failure(RouteErrors.InvalidStopId);
        if (string.IsNullOrWhiteSpace(address)) return Result.Failure(RouteErrors.StopAddressRequired);
        if (address.Trim().Length > RouteStop.AddressMaxLength) return Result.Failure(RouteErrors.StopAddressTooLong);
        if (instructions?.Trim().Length > RouteStop.InstructionsMaxLength) return Result.Failure(RouteErrors.StopInstructionsTooLong);

        return Result.Success();
    }

    private void ChangeStatus(RouteStatus newStatus, DateTimeOffset occurredAtUtc)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new RouteStatusChangedDomainEvent(
            Id,
            CompanyId,
            previousStatus,
            newStatus,
            occurredAtUtc));
    }

    private void ResequenceStops()
    {
        for (var index = 0; index < _stops.Count; index++)
        {
            _stops[index].ChangeSequence(index + 1);
        }
    }

    private static bool IsValidCurrency(string currency)
    {
        var normalizedCurrency = currency.Trim();
        return normalizedCurrency.Length == CurrencyLength &&
            normalizedCurrency.All(char.IsLetter);
    }

    private static string Normalize(string value) => value.Trim();

    private static string NormalizeIdentifier(string value) =>
        value.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCurrency(string? currency) =>
        string.IsNullOrWhiteSpace(currency)
            ? null
            : currency.Trim().ToUpperInvariant();
}
