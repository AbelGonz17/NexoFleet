using NexoFleet.Domain.Common;
using NexoFleet.Domain.RouteSchedules.Events;

namespace NexoFleet.Domain.RouteSchedules;

public sealed class RouteSchedule : AggregateRoot
{
    private readonly List<RouteScheduleDay> _days = [];
    private readonly List<RouteScheduleAssignment> _assignments = [];

    private RouteSchedule(
        Guid id,
        Guid companyId,
        Guid routeId,
        RouteShift shift,
        TimeOnly startTime,
        TimeOnly? endTime,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil,
        decimal? defaultAmount,
        string? defaultCurrency,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        RouteId = routeId;
        Shift = shift;
        StartTime = startTime;
        EndTime = endTime;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        DefaultAmount = defaultAmount;
        DefaultCurrency = defaultCurrency;
        Status = RouteScheduleStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private RouteSchedule()
    {
    }

    public Guid CompanyId { get; private set; }

    public Guid RouteId { get; private set; }

    public RouteShift Shift { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly? EndTime { get; private set; }

    public DateOnly EffectiveFrom { get; private set; }

    public DateOnly? EffectiveUntil { get; private set; }

    public decimal? DefaultAmount { get; private set; }

    public string? DefaultCurrency { get; private set; }

    public RouteScheduleStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<RouteScheduleDay> Days => _days.AsReadOnly();

    public IReadOnlyCollection<RouteScheduleAssignment> Assignments =>
        _assignments.AsReadOnly();

    public static Result<RouteSchedule> Create(
        Guid id,
        Guid companyId,
        Guid routeId,
        RouteShift shift,
        TimeOnly startTime,
        TimeOnly? endTime,
        IEnumerable<DayOfWeek> days,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil,
        decimal? defaultAmount,
        string? defaultCurrency,
        DateTimeOffset createdAtUtc)
    {
        var normalizedDaysResult = NormalizeDays(days);
        if (normalizedDaysResult.IsFailure)
        {
            return Result<RouteSchedule>.Failure(normalizedDaysResult.Error);
        }

        var validationResult = ValidateRecurrence(
            id,
            companyId,
            routeId,
            shift,
            startTime,
            endTime,
            effectiveFrom,
            effectiveUntil,
            defaultAmount,
            defaultCurrency);

        if (validationResult.IsFailure)
        {
            return Result<RouteSchedule>.Failure(validationResult.Error);
        }

        var routeSchedule = new RouteSchedule(
            id,
            companyId,
            routeId,
            shift,
            startTime,
            endTime,
            effectiveFrom,
            effectiveUntil,
            defaultAmount,
            NormalizeCurrency(defaultCurrency),
            createdAtUtc);

        routeSchedule.SetDays(normalizedDaysResult.Value);
        routeSchedule.RaiseDomainEvent(new RouteScheduleCreatedDomainEvent(
            routeSchedule.Id,
            routeSchedule.CompanyId,
            routeSchedule.RouteId,
            createdAtUtc));

        return Result<RouteSchedule>.Success(routeSchedule);
    }

    public Result ConfigureRecurrence(
        RouteShift shift,
        TimeOnly startTime,
        TimeOnly? endTime,
        IEnumerable<DayOfWeek> days,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil,
        decimal? defaultAmount,
        string? defaultCurrency,
        DateTimeOffset updatedAtUtc)
    {
        var normalizedDaysResult = NormalizeDays(days);
        if (normalizedDaysResult.IsFailure)
        {
            return normalizedDaysResult;
        }

        var validationResult = ValidateRecurrence(
            Id,
            CompanyId,
            RouteId,
            shift,
            startTime,
            endTime,
            effectiveFrom,
            effectiveUntil,
            defaultAmount,
            defaultCurrency);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (effectiveUntil.HasValue &&
            _assignments.Any(assignment => !assignment.ValidUntil.HasValue))
        {
            return Result.Failure(RouteScheduleErrors.OpenAssignmentMustBeClosed);
        }

        if (_assignments.Any(assignment =>
                assignment.ValidFrom < effectiveFrom ||
                effectiveUntil.HasValue &&
                (assignment.ValidFrom > effectiveUntil ||
                 assignment.ValidUntil.HasValue && assignment.ValidUntil > effectiveUntil)))
        {
            return Result.Failure(RouteScheduleErrors.AssignmentOutsideSchedulePeriod);
        }

        var normalizedCurrency = NormalizeCurrency(defaultCurrency);
        var currentDays = _days.Select(day => day.DayOfWeek).Order().ToArray();

        if (Shift == shift &&
            StartTime == startTime &&
            EndTime == endTime &&
            EffectiveFrom == effectiveFrom &&
            EffectiveUntil == effectiveUntil &&
            DefaultAmount == defaultAmount &&
            DefaultCurrency == normalizedCurrency &&
            currentDays.SequenceEqual(normalizedDaysResult.Value))
        {
            return Result.Success();
        }

        Shift = shift;
        StartTime = startTime;
        EndTime = endTime;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        DefaultAmount = defaultAmount;
        DefaultCurrency = normalizedCurrency;
        SetDays(normalizedDaysResult.Value);
        UpdatedAtUtc = updatedAtUtc;

        RaiseDomainEvent(new RouteScheduleRecurrenceChangedDomainEvent(
            Id,
            CompanyId,
            RouteId,
            effectiveFrom,
            effectiveUntil,
            updatedAtUtc));

        return Result.Success();
    }

    public Result AssignResources(
        Guid assignmentId,
        Guid employeeId,
        Guid? vehicleId,
        DateOnly validFrom,
        DateOnly? validUntil,
        DateTimeOffset createdAtUtc)
    {
        if (Status == RouteScheduleStatus.Inactive)
        {
            return Result.Failure(RouteScheduleErrors.InactiveScheduleCannotAssign);
        }

        var validationResult = ValidateAssignment(
            assignmentId,
            employeeId,
            vehicleId,
            validFrom,
            validUntil);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (_assignments.Any(assignment => assignment.Id == assignmentId))
        {
            return Result.Failure(RouteScheduleErrors.AssignmentAlreadyExists);
        }

        var openAssignment = _assignments.SingleOrDefault(
            assignment => !assignment.ValidUntil.HasValue);

        if (openAssignment is not null && validFrom <= openAssignment.ValidFrom)
        {
            return Result.Failure(RouteScheduleErrors.AssignmentCannotRewriteHistory);
        }

        if (_assignments.Any(assignment =>
                assignment != openAssignment &&
                PeriodsOverlap(
                    assignment.ValidFrom,
                    assignment.ValidUntil,
                    validFrom,
                    validUntil)))
        {
            return Result.Failure(RouteScheduleErrors.AssignmentPeriodOverlaps);
        }

        if (openAssignment is not null)
        {
            openAssignment.Close(validFrom.AddDays(-1), createdAtUtc);
        }

        var assignment = new RouteScheduleAssignment(
            assignmentId,
            Id,
            CompanyId,
            employeeId,
            vehicleId,
            validFrom,
            validUntil,
            createdAtUtc);
        _assignments.Add(assignment);
        UpdatedAtUtc = createdAtUtc;

        RaiseDomainEvent(new RouteScheduleResourcesAssignedDomainEvent(
            Id,
            CompanyId,
            assignment.Id,
            employeeId,
            vehicleId,
            validFrom,
            validUntil,
            createdAtUtc));

        return Result.Success();
    }

    public Result EndCurrentAssignment(
        DateOnly validUntil,
        DateTimeOffset updatedAtUtc)
    {
        var assignment = _assignments.SingleOrDefault(
            candidate => !candidate.ValidUntil.HasValue);
        if (assignment is null)
        {
            return Result.Failure(RouteScheduleErrors.CurrentAssignmentNotFound);
        }

        if (validUntil < assignment.ValidFrom ||
            EffectiveUntil.HasValue && validUntil > EffectiveUntil)
        {
            return Result.Failure(RouteScheduleErrors.InvalidAssignmentPeriod);
        }

        assignment.Close(validUntil, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public bool OccursOn(DateOnly date) =>
        Status == RouteScheduleStatus.Active &&
        date >= EffectiveFrom &&
        (!EffectiveUntil.HasValue || date <= EffectiveUntil) &&
        _days.Any(day => day.DayOfWeek == date.DayOfWeek);

    public Result Activate(DateTimeOffset occurredAtUtc)
    {
        if (Status == RouteScheduleStatus.Active)
        {
            return Result.Failure(RouteScheduleErrors.AlreadyActive);
        }

        ChangeStatus(RouteScheduleStatus.Active, occurredAtUtc);
        return Result.Success();
    }

    public Result Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (Status == RouteScheduleStatus.Inactive)
        {
            return Result.Failure(RouteScheduleErrors.AlreadyInactive);
        }

        ChangeStatus(RouteScheduleStatus.Inactive, occurredAtUtc);
        return Result.Success();
    }

    private static Result ValidateRecurrence(
        Guid id,
        Guid companyId,
        Guid routeId,
        RouteShift shift,
        TimeOnly startTime,
        TimeOnly? endTime,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil,
        decimal? defaultAmount,
        string? defaultCurrency)
    {
        if (id == Guid.Empty) return Result.Failure(RouteScheduleErrors.InvalidId);
        if (companyId == Guid.Empty) return Result.Failure(RouteScheduleErrors.InvalidCompanyId);
        if (routeId == Guid.Empty) return Result.Failure(RouteScheduleErrors.InvalidRouteId);
        if (!Enum.IsDefined(shift)) return Result.Failure(RouteScheduleErrors.InvalidShift);
        if (endTime == startTime) return Result.Failure(RouteScheduleErrors.EndTimeEqualsStartTime);
        if (effectiveUntil < effectiveFrom) return Result.Failure(RouteScheduleErrors.InvalidEffectivePeriod);
        if (defaultAmount < 0) return Result.Failure(RouteScheduleErrors.InvalidDefaultAmount);
        if (!defaultAmount.HasValue && !string.IsNullOrWhiteSpace(defaultCurrency)) return Result.Failure(RouteScheduleErrors.DefaultAmountRequired);
        if (defaultAmount.HasValue && string.IsNullOrWhiteSpace(defaultCurrency)) return Result.Failure(RouteScheduleErrors.DefaultCurrencyRequired);
        if (!string.IsNullOrWhiteSpace(defaultCurrency) && !IsValidCurrency(defaultCurrency))
        {
            return Result.Failure(RouteScheduleErrors.DefaultCurrencyInvalid);
        }

        return Result.Success();
    }

    private Result ValidateAssignment(
        Guid assignmentId,
        Guid employeeId,
        Guid? vehicleId,
        DateOnly validFrom,
        DateOnly? validUntil)
    {
        if (assignmentId == Guid.Empty) return Result.Failure(RouteScheduleErrors.InvalidAssignmentId);
        if (employeeId == Guid.Empty) return Result.Failure(RouteScheduleErrors.InvalidEmployeeId);
        if (vehicleId == Guid.Empty) return Result.Failure(RouteScheduleErrors.InvalidVehicleId);
        if (validUntil < validFrom) return Result.Failure(RouteScheduleErrors.InvalidAssignmentPeriod);
        if (validFrom < EffectiveFrom ||
            EffectiveUntil.HasValue &&
            (validFrom > EffectiveUntil ||
             validUntil.HasValue && validUntil > EffectiveUntil))
        {
            return Result.Failure(RouteScheduleErrors.AssignmentOutsideSchedulePeriod);
        }

        return Result.Success();
    }

    private static Result<DayOfWeek[]> NormalizeDays(IEnumerable<DayOfWeek>? days)
    {
        if (days is null)
        {
            return Result<DayOfWeek[]>.Failure(RouteScheduleErrors.DaysRequired);
        }

        var normalizedDays = days.Distinct().Order().ToArray();
        if (normalizedDays.Length == 0)
        {
            return Result<DayOfWeek[]>.Failure(RouteScheduleErrors.DaysRequired);
        }

        if (normalizedDays.Any(day => !Enum.IsDefined(day)))
        {
            return Result<DayOfWeek[]>.Failure(RouteScheduleErrors.InvalidDayOfWeek);
        }

        return Result<DayOfWeek[]>.Success(normalizedDays);
    }

    private void SetDays(IReadOnlyCollection<DayOfWeek> days)
    {
        _days.RemoveAll(day => !days.Contains(day.DayOfWeek));

        foreach (var day in days.Where(day =>
                     _days.All(existing => existing.DayOfWeek != day)))
        {
            _days.Add(new RouteScheduleDay(Id, day));
        }

        _days.Sort((left, right) => left.DayOfWeek.CompareTo(right.DayOfWeek));
    }

    private void ChangeStatus(
        RouteScheduleStatus newStatus,
        DateTimeOffset occurredAtUtc)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new RouteScheduleStatusChangedDomainEvent(
            Id,
            CompanyId,
            previousStatus,
            newStatus,
            occurredAtUtc));
    }

    private static bool PeriodsOverlap(
        DateOnly firstFrom,
        DateOnly? firstUntil,
        DateOnly secondFrom,
        DateOnly? secondUntil) =>
        firstFrom <= (secondUntil ?? DateOnly.MaxValue) &&
        secondFrom <= (firstUntil ?? DateOnly.MaxValue);

    private static bool IsValidCurrency(string currency)
    {
        var normalizedCurrency = currency.Trim();
        return normalizedCurrency.Length == RouteScheduleErrors.CurrencyLength &&
            normalizedCurrency.All(char.IsLetter);
    }

    private static string? NormalizeCurrency(string? currency) =>
        string.IsNullOrWhiteSpace(currency)
            ? null
            : currency.Trim().ToUpperInvariant();
}
