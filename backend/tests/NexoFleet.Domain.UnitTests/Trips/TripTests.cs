using NexoFleet.Domain.Routes;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Trips.Events;

namespace NexoFleet.Domain.UnitTests.Trips;

public sealed class TripTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly RouteLocation Origin = RouteLocation.Create("Central Station", -16.5m, -68.15m).Value;
    private static readonly RouteLocation Destination = RouteLocation.Create("Airport", -16.51m, -68.18m).Value;

    [Fact]
    public void EmployeeSubmissionShouldRequireApproval()
    {
        var result = CreateEmployeeTrip();

        Assert.True(result.IsSuccess);
        Assert.Equal(TripSource.Employee, result.Value.Source);
        Assert.Equal(TripStatus.PendingApproval, result.Value.Status);
        Assert.Single(result.Value.StatusHistory);
        Assert.IsType<TripCreatedDomainEvent>(result.Value.DomainEvents.Single());
    }

    [Fact]
    public void ApproveShouldAddReviewAndPlanTrip()
    {
        var trip = CreateEmployeeTrip().Value;

        var result = trip.Approve(Guid.NewGuid(), Guid.NewGuid(), "Valid service", Now.AddMinutes(5));

        Assert.True(result.IsSuccess);
        Assert.Equal(TripStatus.Planned, trip.Status);
        Assert.Single(trip.Reviews);
        Assert.Equal(2, trip.StatusHistory.Count);
    }

    [Fact]
    public void RejectShouldRequireReason()
    {
        var trip = CreateEmployeeTrip().Value;

        var result = trip.Reject(Guid.NewGuid(), Guid.NewGuid(), " ", Now.AddMinutes(5));

        Assert.Equal(TripErrors.ReviewReasonRequired, result.Error);
        Assert.Equal(TripStatus.PendingApproval, trip.Status);
    }

    [Fact]
    public void AssignShouldClosePreviousAssignment()
    {
        var trip = CreatePlannedTrip();
        var first = Guid.NewGuid();

        Assert.True(trip.Assign(first, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now.AddMinutes(1)).IsSuccess);
        Assert.True(trip.Assign(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Now.AddMinutes(2)).IsSuccess);

        Assert.Equal(2, trip.Assignments.Count);
        Assert.NotNull(trip.Assignments.Single(assignment => assignment.Id == first).EndedAtUtc);
        Assert.NotNull(trip.CurrentAssignment);
    }

    [Fact]
    public void AssignedEmployeeShouldStartAndCompleteTrip()
    {
        var trip = CreatePlannedTrip();
        var employeeId = Guid.NewGuid();
        trip.Assign(Guid.NewGuid(), employeeId, null, Guid.NewGuid(), Now.AddMinutes(1));

        var startResult = trip.Start(employeeId, Now.AddMinutes(10));
        var completeResult = trip.Complete(employeeId, 125.50m, "bob", Now.AddHours(1));

        Assert.True(startResult.IsSuccess);
        Assert.True(completeResult.IsSuccess);
        Assert.Equal(TripStatus.Completed, trip.Status);
        Assert.Equal(125.50m, trip.FinalAmount);
        Assert.Equal("BOB", trip.Currency);
        Assert.IsType<TripCompletedDomainEvent>(trip.DomainEvents.Last());
    }

    [Fact]
    public void DifferentEmployeeCannotStartTrip()
    {
        var trip = CreatePlannedTrip();
        trip.Assign(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Now.AddMinutes(1));

        var result = trip.Start(Guid.NewGuid(), Now.AddMinutes(2));

        Assert.Equal(TripErrors.AssignedEmployeeMismatch, result.Error);
    }

    [Fact]
    public void IncidentAndFileShouldBeRegisteredDuringAssignedTrip()
    {
        var trip = CreatePlannedTrip();
        var employeeId = Guid.NewGuid();
        trip.Assign(Guid.NewGuid(), employeeId, null, Guid.NewGuid(), Now.AddMinutes(1));

        var incident = trip.AddIncident(Guid.NewGuid(), employeeId, TripIncidentSeverity.Medium, "Flat tire", Now.AddMinutes(2), Now.AddMinutes(3));
        var file = trip.AddFile(Guid.NewGuid(), "photo.jpg", "trips/photo.jpg", "IMAGE/JPEG", 500, Guid.NewGuid(), Now.AddMinutes(4));

        Assert.True(incident.IsSuccess);
        Assert.True(file.IsSuccess);
        Assert.Single(trip.Incidents);
        Assert.Single(trip.Files);
    }

    [Fact]
    public void CompletedTripCannotBeCancelled()
    {
        var trip = CreatePlannedTrip();
        var employeeId = Guid.NewGuid();
        trip.Assign(Guid.NewGuid(), employeeId, null, Guid.NewGuid(), Now.AddMinutes(1));
        trip.Start(employeeId, Now.AddMinutes(2));
        trip.Complete(employeeId, 100, "USD", Now.AddMinutes(3));

        var result = trip.Cancel("Mistake", Now.AddMinutes(4));

        Assert.Equal(TripErrors.CancellationNotAllowed, result.Error);
    }

    private static NexoFleet.Domain.Common.Result<Trip> CreateEmployeeTrip() => Trip.SubmitUnexpected(
        Guid.NewGuid(), Guid.NewGuid(), " trip-001 ", Guid.NewGuid(), null, null,
        DateOnly.FromDateTime(Now.UtcDateTime), Origin, Destination, 100, "bob", Now);

    private static Trip CreatePlannedTrip() => Trip.CreatePlanned(
        Guid.NewGuid(), Guid.NewGuid(), "trip-002", null, null, null,
        DateOnly.FromDateTime(Now.UtcDateTime), Origin, Destination, 100, "BOB", Now).Value;
}
