using NexoFleet.Domain.Common;
using NexoFleet.Domain.Vehicles.Events;

namespace NexoFleet.Domain.Vehicles;

public sealed class Vehicle : AggregateRoot
{
    private readonly List<VehicleDocument> _documents = [];

    public const int LicensePlateMaxLength = 20;
    public const int MakeMaxLength = 100;
    public const int ModelMaxLength = 100;
    public const int ColorMaxLength = 50;
    public const int MinimumManufactureYear = 1900;

    private Vehicle(
        Guid id,
        Guid companyId,
        Guid? ownerEmployeeId,
        VehicleOwnershipType ownershipType,
        string licensePlate,
        string make,
        string model,
        int manufactureYear,
        string? color,
        VehicleType type,
        int? passengerCapacity,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        OwnerEmployeeId = ownerEmployeeId;
        OwnershipType = ownershipType;
        LicensePlate = licensePlate;
        Make = make;
        Model = model;
        ManufactureYear = manufactureYear;
        Color = color;
        Type = type;
        PassengerCapacity = passengerCapacity;
        Status = VehicleStatus.Operational;
        ApprovalStatus = ownershipType == VehicleOwnershipType.CompanyOwned
            ? VehicleApprovalStatus.NotRequired
            : VehicleApprovalStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    private Vehicle()
    {
    }

    public Guid CompanyId { get; private set; }

    public Guid? OwnerEmployeeId { get; private set; }

    public VehicleOwnershipType OwnershipType { get; private set; }

    public string LicensePlate { get; private set; } = string.Empty;

    public string Make { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public int ManufactureYear { get; private set; }

    public string? Color { get; private set; }

    public VehicleType Type { get; private set; }

    public int? PassengerCapacity { get; private set; }

    public VehicleStatus Status { get; private set; }

    public VehicleApprovalStatus ApprovalStatus { get; private set; }

    public string? ApprovalDecisionReason { get; private set; }

    public DateTimeOffset? ApprovalDecidedAtUtc { get; private set; }

    public bool CanOperate =>
        Status == VehicleStatus.Operational &&
        ApprovalStatus is VehicleApprovalStatus.NotRequired or VehicleApprovalStatus.Approved;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<VehicleDocument> Documents => _documents.AsReadOnly();

    public static Result<Vehicle> CreateCompanyOwned(
        Guid id,
        Guid companyId,
        string licensePlate,
        string make,
        string model,
        int manufactureYear,
        string? color,
        VehicleType type,
        int? passengerCapacity,
        DateTimeOffset createdAtUtc) =>
        Create(
            id,
            companyId,
            null,
            VehicleOwnershipType.CompanyOwned,
            licensePlate,
            make,
            model,
            manufactureYear,
            color,
            type,
            passengerCapacity,
            createdAtUtc);

    public static Result<Vehicle> CreateEmployeeOwned(
        Guid id,
        Guid companyId,
        Guid ownerEmployeeId,
        string licensePlate,
        string make,
        string model,
        int manufactureYear,
        string? color,
        VehicleType type,
        int? passengerCapacity,
        DateTimeOffset createdAtUtc)
    {
        if (ownerEmployeeId == Guid.Empty)
        {
            return Result<Vehicle>.Failure(VehicleErrors.InvalidOwnerEmployeeId);
        }

        return Create(
            id,
            companyId,
            ownerEmployeeId,
            VehicleOwnershipType.EmployeeOwned,
            licensePlate,
            make,
            model,
            manufactureYear,
            color,
            type,
            passengerCapacity,
            createdAtUtc);
    }

    public Result UpdateDetails(
        string licensePlate,
        string make,
        string model,
        int manufactureYear,
        string? color,
        VehicleType type,
        int? passengerCapacity,
        DateTimeOffset updatedAtUtc)
    {
        var validationResult = ValidateDetails(
            Id,
            CompanyId,
            licensePlate,
            make,
            model,
            manufactureYear,
            color,
            type,
            passengerCapacity,
            updatedAtUtc);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var normalizedLicensePlate = NormalizeIdentifier(licensePlate);
        var normalizedMake = Normalize(make);
        var normalizedModel = Normalize(model);
        var normalizedColor = NormalizeOptional(color);

        if (LicensePlate == normalizedLicensePlate &&
            Make == normalizedMake &&
            Model == normalizedModel &&
            ManufactureYear == manufactureYear &&
            Color == normalizedColor &&
            Type == type &&
            PassengerCapacity == passengerCapacity)
        {
            return Result.Success();
        }

        LicensePlate = normalizedLicensePlate;
        Make = normalizedMake;
        Model = normalizedModel;
        ManufactureYear = manufactureYear;
        Color = normalizedColor;
        Type = type;
        PassengerCapacity = passengerCapacity;
        UpdatedAtUtc = updatedAtUtc;

        if (OwnershipType == VehicleOwnershipType.EmployeeOwned &&
            ApprovalStatus is VehicleApprovalStatus.Approved or VehicleApprovalStatus.Rejected)
        {
            ChangeApprovalStatus(
                VehicleApprovalStatus.Pending,
                null,
                updatedAtUtc);
        }

        return Result.Success();
    }

    public Result Approve(DateTimeOffset occurredAtUtc)
    {
        if (ApprovalStatus == VehicleApprovalStatus.NotRequired)
        {
            return Result.Failure(VehicleErrors.ApprovalNotRequired);
        }

        if (ApprovalStatus == VehicleApprovalStatus.Approved)
        {
            return Result.Failure(VehicleErrors.AlreadyApproved);
        }

        if (ApprovalStatus != VehicleApprovalStatus.Pending)
        {
            return Result.Failure(VehicleErrors.ApprovalDecisionRequiresPendingStatus);
        }

        ChangeApprovalStatus(VehicleApprovalStatus.Approved, null, occurredAtUtc);
        return Result.Success();
    }

    public Result Reject(string reason, DateTimeOffset occurredAtUtc)
    {
        var validationResult = ValidateApprovalReason(reason);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (ApprovalStatus == VehicleApprovalStatus.NotRequired)
        {
            return Result.Failure(VehicleErrors.ApprovalNotRequired);
        }

        if (ApprovalStatus == VehicleApprovalStatus.Rejected)
        {
            return Result.Failure(VehicleErrors.AlreadyRejected);
        }

        if (ApprovalStatus != VehicleApprovalStatus.Pending)
        {
            return Result.Failure(VehicleErrors.ApprovalDecisionRequiresPendingStatus);
        }

        ChangeApprovalStatus(
            VehicleApprovalStatus.Rejected,
            Normalize(reason),
            occurredAtUtc);
        return Result.Success();
    }

    public Result AddDocument(
        Guid documentId,
        VehicleDocumentType type,
        string fileName,
        string storageKey,
        string contentType,
        long sizeInBytes,
        DateOnly? expiresOn,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAtUtc)
    {
        if (documentId == Guid.Empty) return Result.Failure(VehicleErrors.InvalidDocumentId);
        if (!Enum.IsDefined(type)) return Result.Failure(VehicleErrors.InvalidDocumentType);
        if (string.IsNullOrWhiteSpace(fileName)) return Result.Failure(VehicleErrors.DocumentFileNameRequired);
        if (string.IsNullOrWhiteSpace(storageKey)) return Result.Failure(VehicleErrors.DocumentStorageKeyRequired);
        if (string.IsNullOrWhiteSpace(contentType)) return Result.Failure(VehicleErrors.DocumentContentTypeRequired);
        if (sizeInBytes <= 0) return Result.Failure(VehicleErrors.InvalidDocumentSize);
        if (uploadedByUserId == Guid.Empty) return Result.Failure(VehicleErrors.InvalidUploadedByUserId);
        if (fileName.Trim().Length > VehicleErrors.DocumentFileNameMaxLength ||
            storageKey.Trim().Length > VehicleErrors.DocumentStorageKeyMaxLength ||
            contentType.Trim().Length > VehicleErrors.DocumentContentTypeMaxLength)
        {
            return Result.Failure(VehicleErrors.DocumentMetadataTooLong);
        }
        if (_documents.Any(document => document.Id == documentId)) return Result.Failure(VehicleErrors.DocumentAlreadyExists);

        _documents.Add(new VehicleDocument(
            documentId,
            Id,
            CompanyId,
            type,
            Normalize(fileName),
            Normalize(storageKey),
            Normalize(contentType).ToLowerInvariant(),
            sizeInBytes,
            expiresOn,
            uploadedByUserId,
            uploadedAtUtc));
        RequireNewApprovalAfterDocumentChange(uploadedAtUtc);
        return Result.Success();
    }

    public Result RemoveDocument(Guid documentId, DateTimeOffset updatedAtUtc)
    {
        var document = _documents.SingleOrDefault(candidate => candidate.Id == documentId);
        if (document is null) return Result.Failure(VehicleErrors.DocumentNotFound);
        _documents.Remove(document);
        RequireNewApprovalAfterDocumentChange(updatedAtUtc);
        return Result.Success();
    }

    public Result SendToMaintenance(DateTimeOffset occurredAtUtc)
    {
        if (Status == VehicleStatus.Retired)
        {
            return Result.Failure(VehicleErrors.RetiredStatusIsFinal);
        }

        if (Status == VehicleStatus.Maintenance)
        {
            return Result.Failure(VehicleErrors.AlreadyInMaintenance);
        }

        ChangeStatus(VehicleStatus.Maintenance, occurredAtUtc);
        return Result.Success();
    }

    public Result ReturnToOperational(DateTimeOffset occurredAtUtc)
    {
        if (Status == VehicleStatus.Retired)
        {
            return Result.Failure(VehicleErrors.RetiredStatusIsFinal);
        }

        if (Status == VehicleStatus.Operational)
        {
            return Result.Failure(VehicleErrors.AlreadyOperational);
        }

        ChangeStatus(VehicleStatus.Operational, occurredAtUtc);
        return Result.Success();
    }

    public Result Retire(DateTimeOffset occurredAtUtc)
    {
        if (Status == VehicleStatus.Retired)
        {
            return Result.Failure(VehicleErrors.AlreadyRetired);
        }

        ChangeStatus(VehicleStatus.Retired, occurredAtUtc);
        return Result.Success();
    }

    private static Result<Vehicle> Create(
        Guid id,
        Guid companyId,
        Guid? ownerEmployeeId,
        VehicleOwnershipType ownershipType,
        string licensePlate,
        string make,
        string model,
        int manufactureYear,
        string? color,
        VehicleType type,
        int? passengerCapacity,
        DateTimeOffset createdAtUtc)
    {
        var validationResult = ValidateDetails(
            id,
            companyId,
            licensePlate,
            make,
            model,
            manufactureYear,
            color,
            type,
            passengerCapacity,
            createdAtUtc);

        if (validationResult.IsFailure)
        {
            return Result<Vehicle>.Failure(validationResult.Error);
        }

        var vehicle = new Vehicle(
            id,
            companyId,
            ownerEmployeeId,
            ownershipType,
            NormalizeIdentifier(licensePlate),
            Normalize(make),
            Normalize(model),
            manufactureYear,
            NormalizeOptional(color),
            type,
            passengerCapacity,
            createdAtUtc);

        vehicle.RaiseDomainEvent(new VehicleCreatedDomainEvent(
            vehicle.Id,
            vehicle.CompanyId,
            vehicle.OwnershipType,
            createdAtUtc));

        return Result<Vehicle>.Success(vehicle);
    }

    private static Result ValidateDetails(
        Guid id,
        Guid companyId,
        string licensePlate,
        string make,
        string model,
        int manufactureYear,
        string? color,
        VehicleType type,
        int? passengerCapacity,
        DateTimeOffset occurredAtUtc)
    {
        if (id == Guid.Empty) return Result.Failure(VehicleErrors.InvalidId);
        if (companyId == Guid.Empty) return Result.Failure(VehicleErrors.InvalidCompanyId);
        if (string.IsNullOrWhiteSpace(licensePlate)) return Result.Failure(VehicleErrors.LicensePlateRequired);
        if (licensePlate.Trim().Length > LicensePlateMaxLength) return Result.Failure(VehicleErrors.LicensePlateTooLong);
        if (string.IsNullOrWhiteSpace(make)) return Result.Failure(VehicleErrors.MakeRequired);
        if (make.Trim().Length > MakeMaxLength) return Result.Failure(VehicleErrors.MakeTooLong);
        if (string.IsNullOrWhiteSpace(model)) return Result.Failure(VehicleErrors.ModelRequired);
        if (model.Trim().Length > ModelMaxLength) return Result.Failure(VehicleErrors.ModelTooLong);
        if (color?.Trim().Length > ColorMaxLength) return Result.Failure(VehicleErrors.ColorTooLong);
        if (!Enum.IsDefined(type)) return Result.Failure(VehicleErrors.InvalidVehicleType);
        if (manufactureYear < MinimumManufactureYear || manufactureYear > occurredAtUtc.UtcDateTime.Year + 1)
        {
            return Result.Failure(VehicleErrors.InvalidManufactureYear);
        }

        if (passengerCapacity <= 0) return Result.Failure(VehicleErrors.InvalidPassengerCapacity);

        return Result.Success();
    }

    private static Result ValidateApprovalReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(VehicleErrors.ApprovalReasonRequired);
        }

        if (reason.Trim().Length > VehicleErrors.ApprovalReasonMaxLength)
        {
            return Result.Failure(VehicleErrors.ApprovalReasonTooLong);
        }

        return Result.Success();
    }

    private void ChangeStatus(VehicleStatus newStatus, DateTimeOffset occurredAtUtc)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new VehicleStatusChangedDomainEvent(
            Id,
            CompanyId,
            previousStatus,
            newStatus,
            occurredAtUtc));
    }

    private void ChangeApprovalStatus(
        VehicleApprovalStatus newStatus,
        string? reason,
        DateTimeOffset occurredAtUtc)
    {
        var previousStatus = ApprovalStatus;
        ApprovalStatus = newStatus;
        ApprovalDecisionReason = reason;
        ApprovalDecidedAtUtc = newStatus is VehicleApprovalStatus.Approved or
            VehicleApprovalStatus.Rejected
                ? occurredAtUtc
                : null;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new VehicleApprovalStatusChangedDomainEvent(
            Id,
            CompanyId,
            previousStatus,
            newStatus,
            reason,
            occurredAtUtc));
    }

    private void RequireNewApprovalAfterDocumentChange(DateTimeOffset occurredAtUtc)
    {
        if (OwnershipType == VehicleOwnershipType.EmployeeOwned &&
            ApprovalStatus is VehicleApprovalStatus.Approved or VehicleApprovalStatus.Rejected)
        {
            ChangeApprovalStatus(VehicleApprovalStatus.Pending, null, occurredAtUtc);
            return;
        }

        UpdatedAtUtc = occurredAtUtc;
    }

    private static string Normalize(string value) => value.Trim();

    private static string NormalizeIdentifier(string value) =>
        value.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
