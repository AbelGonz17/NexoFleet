using NexoFleet.Domain.Common;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Domain.Vehicles.Events;

namespace NexoFleet.Domain.UnitTests.Vehicles;

public sealed class VehicleTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateCompanyOwnedShouldNormalizeDataAndRaiseEvent()
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
        Assert.Equal(VehicleStatus.Available, result.Value.Status);

        var domainEvent = Assert.IsType<VehicleCreatedDomainEvent>(
            result.Value.DomainEvents.Single());
        Assert.Equal(VehicleOwnershipType.CompanyOwned, domainEvent.OwnershipType);
    }

    [Fact]
    public void CreateEmployeeOwnedShouldRequireAndStoreOwner()
    {
        var ownerEmployeeId = Guid.NewGuid();

        var successResult = CreateEmployeeVehicle(ownerEmployeeId);
        var failureResult = CreateEmployeeVehicle(Guid.Empty);

        Assert.True(successResult.IsSuccess);
        Assert.Equal(ownerEmployeeId, successResult.Value.OwnerEmployeeId);
        Assert.Equal(VehicleOwnershipType.EmployeeOwned, successResult.Value.OwnershipType);
        Assert.True(failureResult.IsFailure);
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

        Assert.True(result.IsFailure);
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

        Assert.True(result.IsFailure);
        Assert.Equal(VehicleErrors.InvalidManufactureYear, result.Error);
    }

    [Fact]
    public void CreateShouldAllowUnknownPassengerCapacityButRejectNonPositiveValue()
    {
        var unknownCapacityResult = CreateCompanyVehicle(passengerCapacity: null);
        var invalidCapacityResult = CreateCompanyVehicle(passengerCapacity: 0);

        Assert.True(unknownCapacityResult.IsSuccess);
        Assert.Null(unknownCapacityResult.Value.PassengerCapacity);
        Assert.True(invalidCapacityResult.IsFailure);
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
    public void ServiceCycleShouldMoveFromAvailableToInServiceAndBack()
    {
        var vehicle = CreateCompanyVehicle().Value;
        vehicle.ClearDomainEvents();

        var startResult = vehicle.StartService(Now.AddHours(1));
        var completeResult = vehicle.CompleteService(Now.AddHours(2));

        Assert.True(startResult.IsSuccess);
        Assert.True(completeResult.IsSuccess);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.Equal(2, vehicle.DomainEvents.Count);

        var lastEvent = Assert.IsType<VehicleStatusChangedDomainEvent>(
            vehicle.DomainEvents.Last());
        Assert.Equal(VehicleStatus.InService, lastEvent.PreviousStatus);
        Assert.Equal(VehicleStatus.Available, lastEvent.CurrentStatus);
    }

    [Fact]
    public void MaintenanceVehicleShouldNotStartServiceUntilAvailable()
    {
        var vehicle = CreateCompanyVehicle().Value;

        var maintenanceResult = vehicle.SendToMaintenance(Now.AddHours(1));
        var invalidStartResult = vehicle.StartService(Now.AddHours(2));
        var availableResult = vehicle.ReturnToAvailable(Now.AddHours(3));
        var startResult = vehicle.StartService(Now.AddHours(4));

        Assert.True(maintenanceResult.IsSuccess);
        Assert.Equal(VehicleErrors.MaintenanceVehicleCannotStartService, invalidStartResult.Error);
        Assert.True(availableResult.IsSuccess);
        Assert.True(startResult.IsSuccess);
        Assert.Equal(VehicleStatus.InService, vehicle.Status);
    }

    [Fact]
    public void InServiceVehicleShouldNotEnterMaintenanceOrBeRetired()
    {
        var vehicle = CreateCompanyVehicle().Value;
        vehicle.StartService(Now.AddHours(1));

        var maintenanceResult = vehicle.SendToMaintenance(Now.AddHours(2));
        var retireResult = vehicle.Retire(Now.AddHours(3));

        Assert.Equal(VehicleErrors.InServiceVehicleCannotEnterMaintenance, maintenanceResult.Error);
        Assert.Equal(VehicleErrors.InServiceVehicleCannotBeRetired, retireResult.Error);
        Assert.Equal(VehicleStatus.InService, vehicle.Status);
    }

    [Fact]
    public void RetiredVehicleShouldNotChangeStatus()
    {
        var vehicle = CreateCompanyVehicle().Value;

        var retireResult = vehicle.Retire(Now.AddHours(1));
        var serviceResult = vehicle.StartService(Now.AddHours(2));
        var maintenanceResult = vehicle.SendToMaintenance(Now.AddHours(3));
        var availableResult = vehicle.ReturnToAvailable(Now.AddHours(4));

        Assert.True(retireResult.IsSuccess);
        Assert.Equal(VehicleStatus.Retired, vehicle.Status);
        Assert.Equal(VehicleErrors.RetiredStatusIsFinal, serviceResult.Error);
        Assert.Equal(VehicleErrors.RetiredStatusIsFinal, maintenanceResult.Error);
        Assert.Equal(VehicleErrors.RetiredStatusIsFinal, availableResult.Error);
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
