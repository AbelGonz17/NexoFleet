using NexoFleet.Domain.Common;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.Trips.Events;

namespace NexoFleet.Domain.Trips;

public sealed class Trip : AggregateRoot
{
    private readonly List<TripAssignment> _assignments = [];
    private readonly List<TripStatusHistory> _statusHistory = [];
    private readonly List<TripReview> _reviews = [];
    private readonly List<TripIncident> _incidents = [];
    private readonly List<TripFile> _files = [];

    private Trip(
        Guid id,
        Guid companyId,
        string tripNumber,
        Guid? clientId,
        Guid? routeId,
        Guid? routeScheduleId,
        Guid? submittedByEmployeeId,
        TripSource source,
        DateOnly serviceDate,
        RouteLocation origin,
        RouteLocation destination,
        decimal? agreedAmount,
        string? currency,
        TripStatus status,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        TripNumber = tripNumber;
        ClientId = clientId;
        RouteId = routeId;
        RouteScheduleId = routeScheduleId;
        SubmittedByEmployeeId = submittedByEmployeeId;
        Source = source;
        ServiceDate = serviceDate;
        Origin = origin;
        Destination = destination;
        AgreedAmount = agreedAmount;
        Currency = currency;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    private Trip() { }

    public Guid CompanyId { get; private set; }
    public string TripNumber { get; private set; } = string.Empty;
    public Guid? ClientId { get; private set; }
    public Guid? RouteId { get; private set; }
    public Guid? RouteScheduleId { get; private set; }
    public Guid? SubmittedByEmployeeId { get; private set; }
    public TripSource Source { get; private set; }
    public DateOnly ServiceDate { get; private set; }
    public RouteLocation Origin { get; private set; } = null!;
    public RouteLocation Destination { get; private set; } = null!;
    public decimal? AgreedAmount { get; private set; }
    public decimal? FinalAmount { get; private set; }
    public string? Currency { get; private set; }
    public TripStatus Status { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<TripAssignment> Assignments => _assignments.AsReadOnly();
    public IReadOnlyCollection<TripStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyCollection<TripReview> Reviews => _reviews.AsReadOnly();
    public IReadOnlyCollection<TripIncident> Incidents => _incidents.AsReadOnly();
    public IReadOnlyCollection<TripFile> Files => _files.AsReadOnly();
    public TripAssignment? CurrentAssignment => _assignments.SingleOrDefault(assignment => !assignment.EndedAtUtc.HasValue);

    public static Result<Trip> CreatePlanned(
        Guid id,
        Guid companyId,
        string tripNumber,
        Guid? clientId,
        Guid? routeId,
        Guid? routeScheduleId,
        DateOnly serviceDate,
        RouteLocation origin,
        RouteLocation destination,
        decimal? agreedAmount,
        string? currency,
        DateTimeOffset createdAtUtc) =>
        Create(
            id,
            companyId,
            tripNumber,
            clientId,
            routeId,
            routeScheduleId,
            null,
            routeScheduleId.HasValue ? TripSource.RouteSchedule : TripSource.Administrator,
            serviceDate,
            origin,
            destination,
            agreedAmount,
            currency,
            TripStatus.Planned,
            createdAtUtc);

    public static Result<Trip> SubmitUnexpected(
        Guid id,
        Guid companyId,
        string tripNumber,
        Guid submittedByEmployeeId,
        Guid? clientId,
        Guid? routeId,
        DateOnly serviceDate,
        RouteLocation origin,
        RouteLocation destination,
        decimal? proposedAmount,
        string? currency,
        DateTimeOffset createdAtUtc)
    {
        if (submittedByEmployeeId == Guid.Empty)
        {
            return Result<Trip>.Failure(TripErrors.InvalidEmployeeId);
        }

        return Create(
            id,
            companyId,
            tripNumber,
            clientId,
            routeId,
            null,
            submittedByEmployeeId,
            TripSource.Employee,
            serviceDate,
            origin,
            destination,
            proposedAmount,
            currency,
            TripStatus.PendingApproval,
            createdAtUtc);
    }

    public Result UpdatePlan(
        Guid? clientId,
        Guid? routeId,
        DateOnly serviceDate,
        RouteLocation origin,
        RouteLocation destination,
        decimal? agreedAmount,
        string? currency,
        DateTimeOffset updatedAtUtc)
    {
        if (Status is TripStatus.InProgress or TripStatus.Completed or TripStatus.Cancelled or TripStatus.Rejected)
        {
            return Result.Failure(TripErrors.AssignmentNotAllowed);
        }

        var validation = ValidateDetails(Id, CompanyId, TripNumber, clientId, routeId, RouteScheduleId, origin, destination, agreedAmount, currency);
        if (validation.IsFailure) return validation;

        var normalizedCurrency = NormalizeCurrency(currency);
        if (ClientId == clientId && RouteId == routeId && ServiceDate == serviceDate &&
            Origin == origin && Destination == destination && AgreedAmount == agreedAmount && Currency == normalizedCurrency)
        {
            return Result.Success();
        }

        ClientId = clientId;
        RouteId = routeId;
        ServiceDate = serviceDate;
        Origin = origin;
        Destination = destination;
        AgreedAmount = agreedAmount;
        Currency = normalizedCurrency;
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result Approve(Guid reviewId, Guid reviewerUserId, string? comments, DateTimeOffset reviewedAtUtc)
    {
        var validation = ValidateReview(reviewId, reviewerUserId, comments, false);
        if (validation.IsFailure) return validation;
        if (Status != TripStatus.PendingApproval) return Result.Failure(TripErrors.PendingApprovalRequired);

        _reviews.Add(new TripReview(reviewId, Id, CompanyId, reviewerUserId, TripReviewDecision.Approved, NormalizeOptional(comments), reviewedAtUtc));
        ChangeStatus(TripStatus.Planned, comments, reviewedAtUtc);
        return Result.Success();
    }

    public Result Reject(Guid reviewId, Guid reviewerUserId, string reason, DateTimeOffset reviewedAtUtc)
    {
        var validation = ValidateReview(reviewId, reviewerUserId, reason, true);
        if (validation.IsFailure) return validation;
        if (Status != TripStatus.PendingApproval) return Result.Failure(TripErrors.PendingApprovalRequired);

        _reviews.Add(new TripReview(reviewId, Id, CompanyId, reviewerUserId, TripReviewDecision.Rejected, NormalizeOptional(reason), reviewedAtUtc));
        ChangeStatus(TripStatus.Rejected, reason, reviewedAtUtc);
        return Result.Success();
    }

    public Result Assign(
        Guid assignmentId,
        Guid employeeId,
        Guid? vehicleId,
        Guid assignedByUserId,
        DateTimeOffset assignedAtUtc)
    {
        if (Status is not TripStatus.Planned and not TripStatus.Assigned) return Result.Failure(TripErrors.AssignmentNotAllowed);
        if (assignmentId == Guid.Empty) return Result.Failure(TripErrors.InvalidChildId);
        if (employeeId == Guid.Empty) return Result.Failure(TripErrors.InvalidEmployeeId);
        if (vehicleId == Guid.Empty) return Result.Failure(TripErrors.InvalidVehicleId);
        if (assignedByUserId == Guid.Empty) return Result.Failure(TripErrors.InvalidUserId);
        if (_assignments.Any(assignment => assignment.Id == assignmentId)) return Result.Failure(TripErrors.AssignmentAlreadyExists);
        if (CurrentAssignment is not null && assignedAtUtc <= CurrentAssignment.AssignedAtUtc)
            return Result.Failure(TripErrors.AssignmentCannotRewriteHistory);

        CurrentAssignment?.End(assignedAtUtc);
        _assignments.Add(new TripAssignment(assignmentId, Id, CompanyId, employeeId, vehicleId, assignedByUserId, assignedAtUtc));

        if (Status == TripStatus.Planned)
        {
            ChangeStatus(TripStatus.Assigned, null, assignedAtUtc);
        }
        else
        {
            UpdatedAtUtc = assignedAtUtc;
        }

        RaiseDomainEvent(new TripAssignedDomainEvent(Id, CompanyId, employeeId, vehicleId, assignedAtUtc));
        return Result.Success();
    }

    public Result Start(Guid employeeId, DateTimeOffset startedAtUtc)
    {
        if (Status != TripStatus.Assigned) return Result.Failure(TripErrors.StartNotAllowed);
        var assignment = CurrentAssignment;
        if (assignment is null) return Result.Failure(TripErrors.CurrentAssignmentRequired);
        if (assignment.EmployeeId != employeeId) return Result.Failure(TripErrors.AssignedEmployeeMismatch);
        if (startedAtUtc < assignment.AssignedAtUtc) return Result.Failure(TripErrors.InvalidStartTime);

        StartedAtUtc = startedAtUtc;
        ChangeStatus(TripStatus.InProgress, null, startedAtUtc);
        return Result.Success();
    }

    public Result Complete(
        Guid employeeId,
        decimal finalAmount,
        string currency,
        DateTimeOffset completedAtUtc)
    {
        if (Status != TripStatus.InProgress) return Result.Failure(TripErrors.CompletionNotAllowed);
        var assignment = CurrentAssignment;
        if (assignment is null) return Result.Failure(TripErrors.CurrentAssignmentRequired);
        if (assignment.EmployeeId != employeeId) return Result.Failure(TripErrors.AssignedEmployeeMismatch);
        if (StartedAtUtc.HasValue && completedAtUtc <= StartedAtUtc) return Result.Failure(TripErrors.InvalidCompletionTime);

        var amountValidation = ValidateAmount(finalAmount, currency);
        if (amountValidation.IsFailure) return amountValidation;

        FinalAmount = finalAmount;
        Currency = NormalizeCurrency(currency);
        CompletedAtUtc = completedAtUtc;
        assignment.End(completedAtUtc);
        ChangeStatus(TripStatus.Completed, null, completedAtUtc);
        RaiseDomainEvent(new TripCompletedDomainEvent(Id, CompanyId, employeeId, finalAmount, Currency!, completedAtUtc));
        return Result.Success();
    }

    public Result Cancel(string reason, DateTimeOffset cancelledAtUtc)
    {
        if (Status is TripStatus.InProgress or TripStatus.Completed or TripStatus.Cancelled or TripStatus.Rejected)
        {
            return Result.Failure(TripErrors.CancellationNotAllowed);
        }
        if (string.IsNullOrWhiteSpace(reason)) return Result.Failure(TripErrors.CancellationReasonRequired);
        if (reason.Trim().Length > TripErrors.NotesMaxLength) return Result.Failure(TripErrors.NotesTooLong);

        CancellationReason = Normalize(reason);
        CurrentAssignment?.End(cancelledAtUtc);
        ChangeStatus(TripStatus.Cancelled, reason, cancelledAtUtc);
        return Result.Success();
    }

    public Result AddIncident(
        Guid incidentId,
        Guid reportedByEmployeeId,
        TripIncidentSeverity severity,
        string description,
        DateTimeOffset incidentAtUtc,
        DateTimeOffset createdAtUtc)
    {
        if (Status is not TripStatus.Assigned and not TripStatus.InProgress and not TripStatus.Completed)
            return Result.Failure(TripErrors.IncidentNotAllowed);
        if (incidentId == Guid.Empty) return Result.Failure(TripErrors.InvalidChildId);
        if (reportedByEmployeeId == Guid.Empty) return Result.Failure(TripErrors.InvalidEmployeeId);
        if (!Enum.IsDefined(severity)) return Result.Failure(TripErrors.InvalidIncidentSeverity);
        if (string.IsNullOrWhiteSpace(description)) return Result.Failure(TripErrors.IncidentDescriptionRequired);
        if (description.Trim().Length > TripErrors.IncidentDescriptionMaxLength) return Result.Failure(TripErrors.IncidentDescriptionTooLong);
        if (_incidents.Any(incident => incident.Id == incidentId)) return Result.Failure(TripErrors.ChildAlreadyExists);

        _incidents.Add(new TripIncident(incidentId, Id, CompanyId, reportedByEmployeeId, severity, Normalize(description), incidentAtUtc, createdAtUtc));
        UpdatedAtUtc = createdAtUtc;
        return Result.Success();
    }

    public Result AddFile(
        Guid fileId,
        string fileName,
        string storageKey,
        string contentType,
        long sizeInBytes,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAtUtc)
    {
        if (fileId == Guid.Empty) return Result.Failure(TripErrors.InvalidChildId);
        if (uploadedByUserId == Guid.Empty) return Result.Failure(TripErrors.InvalidUserId);
        if (string.IsNullOrWhiteSpace(fileName)) return Result.Failure(TripErrors.FileNameRequired);
        if (string.IsNullOrWhiteSpace(storageKey)) return Result.Failure(TripErrors.StorageKeyRequired);
        if (string.IsNullOrWhiteSpace(contentType)) return Result.Failure(TripErrors.ContentTypeRequired);
        if (sizeInBytes <= 0) return Result.Failure(TripErrors.InvalidFileSize);
        if (fileName.Trim().Length > TripErrors.FileNameMaxLength || storageKey.Trim().Length > TripErrors.StorageKeyMaxLength || contentType.Trim().Length > TripErrors.ContentTypeMaxLength)
            return Result.Failure(TripErrors.FileMetadataTooLong);
        if (_files.Any(file => file.Id == fileId)) return Result.Failure(TripErrors.ChildAlreadyExists);

        _files.Add(new TripFile(fileId, Id, CompanyId, Normalize(fileName), Normalize(storageKey), Normalize(contentType).ToLowerInvariant(), sizeInBytes, uploadedByUserId, uploadedAtUtc));
        UpdatedAtUtc = uploadedAtUtc;
        return Result.Success();
    }

    private static Result<Trip> Create(
        Guid id,
        Guid companyId,
        string tripNumber,
        Guid? clientId,
        Guid? routeId,
        Guid? routeScheduleId,
        Guid? submittedByEmployeeId,
        TripSource source,
        DateOnly serviceDate,
        RouteLocation origin,
        RouteLocation destination,
        decimal? amount,
        string? currency,
        TripStatus status,
        DateTimeOffset createdAtUtc)
    {
        var validation = ValidateDetails(id, companyId, tripNumber, clientId, routeId, routeScheduleId, origin, destination, amount, currency);
        if (validation.IsFailure) return Result<Trip>.Failure(validation.Error);

        var trip = new Trip(id, companyId, NormalizeIdentifier(tripNumber), clientId, routeId, routeScheduleId,
            submittedByEmployeeId, source, serviceDate, origin, destination, amount, NormalizeCurrency(currency), status, createdAtUtc);
        trip._statusHistory.Add(new TripStatusHistory(Guid.NewGuid(), id, companyId, null, status, null, createdAtUtc));
        trip.RaiseDomainEvent(new TripCreatedDomainEvent(id, companyId, source, createdAtUtc));
        return Result<Trip>.Success(trip);
    }

    private static Result ValidateDetails(
        Guid id,
        Guid companyId,
        string tripNumber,
        Guid? clientId,
        Guid? routeId,
        Guid? routeScheduleId,
        RouteLocation? origin,
        RouteLocation? destination,
        decimal? amount,
        string? currency)
    {
        if (id == Guid.Empty) return Result.Failure(TripErrors.InvalidId);
        if (companyId == Guid.Empty) return Result.Failure(TripErrors.InvalidCompanyId);
        if (clientId == Guid.Empty) return Result.Failure(TripErrors.InvalidClientId);
        if (routeId == Guid.Empty) return Result.Failure(TripErrors.InvalidRouteId);
        if (routeScheduleId == Guid.Empty) return Result.Failure(TripErrors.InvalidRouteScheduleId);
        if (routeScheduleId.HasValue && !routeId.HasValue) return Result.Failure(TripErrors.RouteRequiredForSchedule);
        if (string.IsNullOrWhiteSpace(tripNumber)) return Result.Failure(TripErrors.TripNumberRequired);
        if (tripNumber.Trim().Length > TripErrors.TripNumberMaxLength) return Result.Failure(TripErrors.TripNumberTooLong);
        if (origin is null) return Result.Failure(TripErrors.OriginRequired);
        if (destination is null) return Result.Failure(TripErrors.DestinationRequired);
        return ValidateOptionalAmount(amount, currency);
    }

    private static Result ValidateReview(Guid reviewId, Guid reviewerUserId, string? comments, bool reasonRequired)
    {
        if (reviewId == Guid.Empty) return Result.Failure(TripErrors.InvalidChildId);
        if (reviewerUserId == Guid.Empty) return Result.Failure(TripErrors.InvalidUserId);
        if (reasonRequired && string.IsNullOrWhiteSpace(comments)) return Result.Failure(TripErrors.ReviewReasonRequired);
        if (comments?.Trim().Length > TripErrors.NotesMaxLength) return Result.Failure(TripErrors.NotesTooLong);
        return Result.Success();
    }

    private static Result ValidateOptionalAmount(decimal? amount, string? currency)
    {
        if (amount < 0) return Result.Failure(TripErrors.InvalidAmount);
        if (!amount.HasValue && !string.IsNullOrWhiteSpace(currency)) return Result.Failure(TripErrors.AmountRequired);
        if (amount.HasValue && string.IsNullOrWhiteSpace(currency)) return Result.Failure(TripErrors.CurrencyRequired);
        if (!string.IsNullOrWhiteSpace(currency) && !IsValidCurrency(currency)) return Result.Failure(TripErrors.CurrencyInvalid);
        return Result.Success();
    }

    private static Result ValidateAmount(decimal amount, string currency) => ValidateOptionalAmount(amount, currency);

    private void ChangeStatus(TripStatus newStatus, string? notes, DateTimeOffset occurredAtUtc)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAtUtc;
        _statusHistory.Add(new TripStatusHistory(Guid.NewGuid(), Id, CompanyId, previousStatus, newStatus, NormalizeOptional(notes), occurredAtUtc));
        RaiseDomainEvent(new TripStatusChangedDomainEvent(Id, CompanyId, previousStatus, newStatus, occurredAtUtc));
    }

    private static bool IsValidCurrency(string currency) => currency.Trim().Length == TripErrors.CurrencyLength && currency.Trim().All(char.IsLetter);
    private static string Normalize(string value) => value.Trim();
    private static string NormalizeIdentifier(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeCurrency(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
