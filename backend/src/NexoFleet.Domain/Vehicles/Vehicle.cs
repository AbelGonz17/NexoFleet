using NexoFleet.Domain.Common;
using NexoFleet.Domain.Vehicles.Events;

namespace NexoFleet.Domain.Vehicles;

public sealed class Vehicle : AggregateRoot
{
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
        Status = VehicleStatus.Available;
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

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

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

        return Result.Success();
    }

    public Result StartService(DateTimeOffset occurredAtUtc)
    {
        if (Status == VehicleStatus.Retired)
        {
            return Result.Failure(VehicleErrors.RetiredStatusIsFinal);
        }

        if (Status == VehicleStatus.InService)
        {
            return Result.Failure(VehicleErrors.AlreadyInService);
        }

        if (Status == VehicleStatus.Maintenance)
        {
            return Result.Failure(VehicleErrors.MaintenanceVehicleCannotStartService);
        }

        ChangeStatus(VehicleStatus.InService, occurredAtUtc);
        return Result.Success();
    }

    public Result CompleteService(DateTimeOffset occurredAtUtc)
    {
        if (Status != VehicleStatus.InService)
        {
            return Result.Failure(VehicleErrors.NotInService);
        }

        ChangeStatus(VehicleStatus.Available, occurredAtUtc);
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

        if (Status == VehicleStatus.InService)
        {
            return Result.Failure(VehicleErrors.InServiceVehicleCannotEnterMaintenance);
        }

        ChangeStatus(VehicleStatus.Maintenance, occurredAtUtc);
        return Result.Success();
    }

    public Result ReturnToAvailable(DateTimeOffset occurredAtUtc)
    {
        if (Status == VehicleStatus.Retired)
        {
            return Result.Failure(VehicleErrors.RetiredStatusIsFinal);
        }

        if (Status == VehicleStatus.Available)
        {
            return Result.Failure(VehicleErrors.AlreadyAvailable);
        }

        if (Status == VehicleStatus.InService)
        {
            return Result.Failure(VehicleErrors.InServiceVehicleCannotBeMarkedAvailable);
        }

        ChangeStatus(VehicleStatus.Available, occurredAtUtc);
        return Result.Success();
    }

    public Result Retire(DateTimeOffset occurredAtUtc)
    {
        if (Status == VehicleStatus.Retired)
        {
            return Result.Failure(VehicleErrors.AlreadyRetired);
        }

        if (Status == VehicleStatus.InService)
        {
            return Result.Failure(VehicleErrors.InServiceVehicleCannotBeRetired);
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
        if (manufactureYear < MinimumManufactureYear || manufactureYear > occurredAtUtc.UtcDateTime.Year + 1)
        {
            return Result.Failure(VehicleErrors.InvalidManufactureYear);
        }

        if (passengerCapacity <= 0) return Result.Failure(VehicleErrors.InvalidPassengerCapacity);

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

    private static string Normalize(string value) => value.Trim();

    private static string NormalizeIdentifier(string value) =>
        value.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
