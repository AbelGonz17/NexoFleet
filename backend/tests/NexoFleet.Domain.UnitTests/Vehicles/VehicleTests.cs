using NexoFleet.Domain.Common;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Domain.Vehicles.Events;

namespace NexoFleet.Domain.UnitTests.Vehicles;

public sealed class VehicleTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void VehicleDocumentShouldBeAddedAndRemoved()
    {
        var vehicle = CreateCompanyVehicle().Value;
        var documentId = Guid.NewGuid();

        var addResult = vehicle.AddDocument(
            documentId,
            VehicleDocumentType.Insurance,
            " insurance.pdf ",
            " vehicles/insurance.pdf ",
            " APPLICATION/PDF ",
            1000,
            new DateOnly(2027, 8, 27),
            Guid.NewGuid(),
            Now.AddMinutes(1));
        var removeResult = vehicle.RemoveDocument(documentId, Now.AddMinutes(2));

        Assert.True(addResult.IsSuccess);
        Assert.True(removeResult.IsSuccess);
        Assert.Empty(vehicle.Documents);
    }

    [Fact]
    public void CreateCompanyOwnedShouldBeOperationalWithoutApproval()
    {
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var result = CreateCompanyVehicle(id, companyId);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Null(result.Value.OwnerEmployeeId);
        Assert.Equal(VehicleOwnershipType.CompanyOwned, result.Value.OwnershipType);
        Assert.Equal("ABC-123", result.Value.LicensePlate);
        Assert.Equal("Toyota", result.Value.Make);
        Assert.Equal("Coaster", result.Value.Model);
        Assert.Equal("Blanco", result.Value.Color);
        Assert.Equal(VehicleStatus.Operational, result.Value.Status);
        Assert.Equal(VehicleApprovalStatus.NotRequired, result.Value.ApprovalStatus);
        Assert.True(result.Value.CanOperate);

        var domainEvent = Assert.IsType<VehicleCreatedDomainEvent>(
            result.Value.DomainEvents.Single());
        Assert.Equal(VehicleOwnershipType.CompanyOwned, domainEvent.OwnershipType);
    }

    [Fact]
    public void CreateEmployeeOwnedShouldRequireOwnerAndStartPending()
    {
        var ownerEmployeeId = Guid.NewGuid();

        var successResult = CreateEmployeeVehicle(ownerEmployeeId);
        var failureResult = CreateEmployeeVehicle(Guid.Empty);

        Assert.True(successResult.IsSuccess);
        Assert.Equal(ownerEmployeeId, successResult.Value.OwnerEmployeeId);
        Assert.Equal(VehicleOwnershipType.EmployeeOwned, successResult.Value.OwnershipType);
        Assert.Equal(VehicleApprovalStatus.Pending, successResult.Value.ApprovalStatus);
        Assert.False(successResult.Value.CanOperate);
        Assert.Equal(VehicleErrors.InvalidOwnerEmployeeId, failureResult.Error);
    }

    [Theory]
    [InlineData("", "Toyota", "Coaster", "Vehicle.LicensePlateRequired")]
    [InlineData("ABC-123", "", "Coaster", "Vehicle.MakeRequired")]
    [InlineData("ABC-123", "Toyota", "", "Vehicle.ModelRequired")]
    public void CreateShouldRejectRequiredInvalidDetails(
        string licensePlate,
        string make,
        string model,
        string expectedErrorCode)
    {
        var result = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            Guid.NewGuid(),
            licensePlate,
            make,
            model,
            2024,
            "Blanco",
            VehicleType.Minibus,
            24,
            Now);

        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2028)]
    public void CreateShouldRejectInvalidManufactureYear(int manufactureYear)
    {
        var result = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC-123",
            "Toyota",
            "Coaster",
            manufactureYear,
            null,
            VehicleType.Minibus,
            24,
            Now);

        Assert.Equal(VehicleErrors.InvalidManufactureYear, result.Error);
    }

    [Fact]
    public void CreateShouldRejectUndefinedVehicleType()
    {
        var result = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC-123",
            "Toyota",
            "Coaster",
            2024,
            null,
            (VehicleType)999,
            24,
            Now);

        Assert.Equal(VehicleErrors.InvalidVehicleType, result.Error);
    }

    [Fact]
    public void CreateShouldAllowUnknownPassengerCapacityButRejectNonPositiveValue()
    {
        var unknownCapacityResult = CreateCompanyVehicle(passengerCapacity: null);
        var invalidCapacityResult = CreateCompanyVehicle(passengerCapacity: 0);

        Assert.True(unknownCapacityResult.IsSuccess);
        Assert.Null(unknownCapacityResult.Value.PassengerCapacity);
        Assert.Equal(VehicleErrors.InvalidPassengerCapacity, invalidCapacityResult.Error);
    }

    [Fact]
    public void UpdateDetailsShouldNormalizeAndAvoidRedundantUpdates()
    {
        var vehicle = CreateCompanyVehicle().Value;
        vehicle.ClearDomainEvents();

        var updateResult = vehicle.UpdateDetails(
            " xyz-789 ",
            " Mercedes-Benz ",
            " Sprinter ",
            2025,
            " Gris ",
            VehicleType.Van,
            18,
            Now.AddHours(1));

        var unchangedResult = vehicle.UpdateDetails(
            vehicle.LicensePlate,
            vehicle.Make,
            vehicle.Model,
            vehicle.ManufactureYear,
            vehicle.Color,
            vehicle.Type,
            vehicle.PassengerCapacity,
            Now.AddHours(2));

        Assert.True(updateResult.IsSuccess);
        Assert.True(unchangedResult.IsSuccess);
        Assert.Equal("XYZ-789", vehicle.LicensePlate);
        Assert.Equal("Mercedes-Benz", vehicle.Make);
        Assert.Equal(Now.AddHours(1), vehicle.UpdatedAtUtc);
        Assert.Empty(vehicle.DomainEvents);
    }

    [Fact]
    public void EmployeeOwnedVehicleShouldRequireApprovalToOperate()
    {
        var vehicle = CreateEmployeeVehicle(Guid.NewGuid()).Value;
        vehicle.ClearDomainEvents();

        var approveResult = vehicle.Approve(Now.AddHours(1));

        Assert.True(approveResult.IsSuccess);
        Assert.Equal(VehicleApprovalStatus.Approved, vehicle.ApprovalStatus);
        Assert.Equal(Now.AddHours(1), vehicle.ApprovalDecidedAtUtc);
        Assert.True(vehicle.CanOperate);
        Assert.IsType<VehicleApprovalStatusChangedDomainEvent>(
            vehicle.DomainEvents.Single());
    }

    [Fact]
    public void CompanyOwnedVehicleShouldNotEnterApprovalWorkflow()
    {
        var vehicle = CreateCompanyVehicle().Value;

        var result = vehicle.Approve(Now.AddHours(1));

        Assert.Equal(VehicleErrors.ApprovalNotRequired, result.Error);
        Assert.Equal(VehicleApprovalStatus.NotRequired, vehicle.ApprovalStatus);
    }

    [Fact]
    public void RejectedVehicleShouldReturnToPendingAfterCorrection()
    {
        var vehicle = CreateEmployeeVehicle(Guid.NewGuid()).Value;
        vehicle.Reject("El vehículo no cumple los requisitos.", Now.AddHours(1));
        vehicle.ClearDomainEvents();

        var updateResult = vehicle.UpdateDetails(
            "new-789",
            vehicle.Make,
            vehicle.Model,
            vehicle.ManufactureYear,
            vehicle.Color,
            vehicle.Type,
            vehicle.PassengerCapacity,
            Now.AddHours(2));

        Assert.True(updateResult.IsSuccess);
        Assert.Equal(VehicleApprovalStatus.Pending, vehicle.ApprovalStatus);
        Assert.Null(vehicle.ApprovalDecisionReason);
        Assert.Null(vehicle.ApprovalDecidedAtUtc);
        Assert.False(vehicle.CanOperate);

        var domainEvent = Assert.IsType<VehicleApprovalStatusChangedDomainEvent>(
            vehicle.DomainEvents.Single());
        Assert.Equal(VehicleApprovalStatus.Rejected, domainEvent.PreviousStatus);
        Assert.Equal(VehicleApprovalStatus.Pending, domainEvent.CurrentStatus);
    }

    [Fact]
    public void UpdatingApprovedEmployeeVehicleShouldRequireNewApproval()
    {
        var vehicle = CreateEmployeeVehicle(Guid.NewGuid()).Value;
        vehicle.Approve(Now.AddHours(1));
        vehicle.ClearDomainEvents();

        var updateResult = vehicle.UpdateDetails(
            "new-789",
            vehicle.Make,
            vehicle.Model,
            vehicle.ManufactureYear,
            vehicle.Color,
            vehicle.Type,
            vehicle.PassengerCapacity,
            Now.AddHours(2));

        Assert.True(updateResult.IsSuccess);
        Assert.Equal(VehicleApprovalStatus.Pending, vehicle.ApprovalStatus);
        Assert.False(vehicle.CanOperate);
    }

    [Fact]
    public void MaintenanceCycleShouldControlOperationalState()
    {
        var vehicle = CreateCompanyVehicle().Value;
        vehicle.ClearDomainEvents();

        var maintenanceResult = vehicle.SendToMaintenance(Now.AddHours(1));
        var operationalResult = vehicle.ReturnToOperational(Now.AddHours(2));

        Assert.True(maintenanceResult.IsSuccess);
        Assert.True(operationalResult.IsSuccess);
        Assert.Equal(VehicleStatus.Operational, vehicle.Status);
        Assert.True(vehicle.CanOperate);
        Assert.Equal(2, vehicle.DomainEvents.Count);

        var lastEvent = Assert.IsType<VehicleStatusChangedDomainEvent>(
            vehicle.DomainEvents.Last());
        Assert.Equal(VehicleStatus.Maintenance, lastEvent.PreviousStatus);
        Assert.Equal(VehicleStatus.Operational, lastEvent.CurrentStatus);
    }

    [Fact]
    public void RetiredVehicleShouldNotReturnToAnotherStatus()
    {
        var vehicle = CreateCompanyVehicle().Value;

        var retireResult = vehicle.Retire(Now.AddHours(1));
        var maintenanceResult = vehicle.SendToMaintenance(Now.AddHours(2));
        var operationalResult = vehicle.ReturnToOperational(Now.AddHours(3));

        Assert.True(retireResult.IsSuccess);
        Assert.Equal(VehicleStatus.Retired, vehicle.Status);
        Assert.Equal(VehicleErrors.RetiredStatusIsFinal, maintenanceResult.Error);
        Assert.Equal(VehicleErrors.RetiredStatusIsFinal, operationalResult.Error);
        Assert.False(vehicle.CanOperate);
    }

    private static Result<Vehicle> CreateCompanyVehicle(
        Guid? id = null,
        Guid? companyId = null,
        int? passengerCapacity = 24) =>
        Vehicle.CreateCompanyOwned(
            id ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            " abc-123 ",
            " Toyota ",
            " Coaster ",
            2024,
            " Blanco ",
            VehicleType.Minibus,
            passengerCapacity,
            Now);

    private static Result<Vehicle> CreateEmployeeVehicle(Guid ownerEmployeeId) =>
        Vehicle.CreateEmployeeOwned(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ownerEmployeeId,
            " emp-456 ",
            " Nissan ",
            " Urvan ",
            2023,
            " Plata ",
            VehicleType.Van,
            15,
            Now);
}
