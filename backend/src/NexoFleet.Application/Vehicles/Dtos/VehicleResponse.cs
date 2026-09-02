using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles.Dtos;

public sealed record VehicleResponse(
    Guid Id,
    Guid CompanyId,
    Guid? OwnerEmployeeId,
    string OwnershipType,
    string LicensePlate,
    string Make,
    string Model,
    int ManufactureYear,
    string? Color,
    string Type,
    int? PassengerCapacity,
    string Status,
    string ApprovalStatus,
    string? ApprovalDecisionReason,
    DateTimeOffset? ApprovalDecidedAtUtc,
    bool CanOperate,
    IReadOnlyList<VehicleDocumentResponse> Documents,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static VehicleResponse FromDomain(Vehicle vehicle) =>
        new(
            vehicle.Id,
            vehicle.CompanyId,
            vehicle.OwnerEmployeeId,
            vehicle.OwnershipType.ToString(),
            vehicle.LicensePlate,
            vehicle.Make,
            vehicle.Model,
            vehicle.ManufactureYear,
            vehicle.Color,
            vehicle.Type.ToString(),
            vehicle.PassengerCapacity,
            vehicle.Status.ToString(),
            vehicle.ApprovalStatus.ToString(),
            vehicle.ApprovalDecisionReason,
            vehicle.ApprovalDecidedAtUtc,
            vehicle.CanOperate,
            vehicle.Documents.Select(VehicleDocumentResponse.FromDomain).ToArray(),
            vehicle.CreatedAtUtc,
            vehicle.UpdatedAtUtc);
}
