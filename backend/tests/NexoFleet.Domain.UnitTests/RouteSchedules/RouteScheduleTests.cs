using NexoFleet.Domain.Common;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.RouteSchedules.Events;

namespace NexoFleet.Domain.UnitTests.RouteSchedules;

public sealed class RouteScheduleTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly EffectiveFrom = new(2026, 9, 1);
    private static readonly DateOnly EffectiveUntil = new(2026, 9, 30);

    [Fact]
    public void CreateShouldNormalizeDaysAndCurrencyAndRaiseEvent()
    {
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var routeId = Guid.NewGuid();

        var result = CreateSchedule(
            id,
            companyId,
            routeId,
            [DayOfWeek.Friday, DayOfWeek.Monday, DayOfWeek.Monday]);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Equal(routeId, result.Value.RouteId);
        Assert.Equal(RouteShift.Morning, result.Value.Shift);
        Assert.Equal(new TimeOnly(6, 30), result.Value.StartTime);
        Assert.Equal("BOB", result.Value.DefaultCurrency);
        Assert.Equal(RouteScheduleStatus.Active, result.Value.Status);
        Assert.Collection(
            result.Value.Days,
            day => Assert.Equal(DayOfWeek.Monday, day.DayOfWeek),
            day => Assert.Equal(DayOfWeek.Friday, day.DayOfWeek));

        var domainEvent = Assert.IsType<RouteScheduleCreatedDomainEvent>(
            result.Value.DomainEvents.Single());
        Assert.Equal(id, domainEvent.RouteScheduleId);
        Assert.Equal(routeId, domainEvent.RouteId);
    }

    [Fact]
    public void CreateShouldRequireAtLeastOneDay()
    {
        var result = CreateSchedule(days: []);

        Assert.Equal(RouteScheduleErrors.DaysRequired, result.Error);
    }

    [Fact]
    public void CreateShouldRejectInvalidEffectivePeriod()
    {
        var result = RouteSchedule.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            RouteShift.Morning,
            new TimeOnly(6, 30),
            null,
            [DayOfWeek.Monday],
            EffectiveUntil,
            EffectiveFrom,
            null,
            null,
            Now);

        Assert.Equal(RouteScheduleErrors.InvalidEffectivePeriod, result.Error);
    }

    [Fact]
    public void EndTimeMayCrossMidnightButCannotEqualStartTime()
    {
        var overnightResult = RouteSchedule.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            RouteShift.Night,
            new TimeOnly(22, 0),
            new TimeOnly(1, 0),
            [DayOfWeek.Monday],
            EffectiveFrom,
            EffectiveUntil,
            null,
            null,
            Now);
        var equalTimeResult = RouteSchedule.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            RouteShift.Night,
            new TimeOnly(22, 0),
            new TimeOnly(22, 0),
            [DayOfWeek.Monday],
            EffectiveFrom,
            EffectiveUntil,
            null,
            null,
            Now);

        Assert.True(overnightResult.IsSuccess);
        Assert.Equal(RouteScheduleErrors.EndTimeEqualsStartTime, equalTimeResult.Error);
    }

    [Theory]
    [InlineData(null, "BOB", "RouteSchedule.DefaultAmountRequired")]
    [InlineData(150.0, null, "RouteSchedule.DefaultCurrencyRequired")]
    [InlineData(150.0, "BO", "RouteSchedule.DefaultCurrencyInvalid")]
    [InlineData(-1.0, "BOB", "RouteSchedule.InvalidDefaultAmount")]
    public void DefaultAmountAndCurrencyShouldBeConsistent(
        double? amount,
        string? currency,
        string expectedErrorCode)
    {
        var result = RouteSchedule.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            RouteShift.Morning,
            new TimeOnly(6, 30),
            null,
            [DayOfWeek.Monday],
            EffectiveFrom,
            EffectiveUntil,
            amount.HasValue ? (decimal)amount.Value : null,
            currency,
            Now);

        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public void ConfigureRecurrenceShouldReplaceDaysWithoutDuplicates()
    {
        var schedule = CreateSchedule().Value;
        schedule.ClearDomainEvents();

        var updateResult = schedule.ConfigureRecurrence(
            RouteShift.Night,
            new TimeOnly(19, 0),
            new TimeOnly(20, 0),
            [DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Thursday],
            EffectiveFrom,
            EffectiveUntil,
            175m,
            " usd ",
            Now.AddHours(1));

        Assert.True(updateResult.IsSuccess);
        Assert.Equal(RouteShift.Night, schedule.Shift);
        Assert.Equal("USD", schedule.DefaultCurrency);
        Assert.Collection(
            schedule.Days,
            day => Assert.Equal(DayOfWeek.Tuesday, day.DayOfWeek),
            day => Assert.Equal(DayOfWeek.Thursday, day.DayOfWeek));
        Assert.Equal(Now.AddHours(1), schedule.UpdatedAtUtc);
        Assert.IsType<RouteScheduleRecurrenceChangedDomainEvent>(
            schedule.DomainEvents.Single());
    }

    [Fact]
    public void OccursOnShouldRespectDayValidityAndStatus()
    {
        var schedule = CreateSchedule(days: [DayOfWeek.Tuesday]).Value;

        Assert.True(schedule.OccursOn(new DateOnly(2026, 9, 1)));
        Assert.False(schedule.OccursOn(new DateOnly(2026, 9, 2)));
        Assert.False(schedule.OccursOn(new DateOnly(2026, 10, 6)));

        schedule.Deactivate(Now.AddHours(1));

        Assert.False(schedule.OccursOn(new DateOnly(2026, 9, 8)));
    }

    [Fact]
    public void AssignResourcesShouldCreateAssignmentAndPublishEvent()
    {
        var schedule = CreateSchedule().Value;
        schedule.ClearDomainEvents();
        var assignmentId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var result = schedule.AssignResources(
            assignmentId,
            employeeId,
            vehicleId,
            EffectiveFrom,
            null,
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        var assignment = Assert.Single(schedule.Assignments);
        Assert.Equal(assignmentId, assignment.Id);
        Assert.Equal(schedule.CompanyId, assignment.CompanyId);
        Assert.Equal(employeeId, assignment.EmployeeId);
        Assert.Equal(vehicleId, assignment.VehicleId);
        Assert.Null(assignment.ValidUntil);

        var domainEvent = Assert.IsType<RouteScheduleResourcesAssignedDomainEvent>(
            schedule.DomainEvents.Single());
        Assert.Equal(assignmentId, domainEvent.AssignmentId);
    }

    [Fact]
    public void NewAssignmentShouldCloseCurrentAssignmentWithoutRewritingHistory()
    {
        var schedule = CreateSchedule().Value;
        schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            EffectiveFrom,
            null,
            Now.AddHours(1));

        var result = schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 15),
            null,
            Now.AddHours(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, schedule.Assignments.Count);
        var previous = schedule.Assignments.OrderBy(assignment => assignment.ValidFrom).First();
        var current = schedule.Assignments.OrderBy(assignment => assignment.ValidFrom).Last();
        Assert.Equal(new DateOnly(2026, 9, 14), previous.ValidUntil);
        Assert.Null(current.ValidUntil);
    }

    [Fact]
    public void AssignmentShouldNotOverlapExistingHistory()
    {
        var schedule = CreateSchedule().Value;
        schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 20),
            Now.AddHours(1));

        var result = schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            new DateOnly(2026, 9, 15),
            new DateOnly(2026, 9, 25),
            Now.AddHours(2));

        Assert.Equal(RouteScheduleErrors.AssignmentPeriodOverlaps, result.Error);
        Assert.Single(schedule.Assignments);
    }

    [Fact]
    public void AssignmentShouldRemainInsideScheduleValidity()
    {
        var schedule = CreateSchedule().Value;

        var beforeResult = schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            EffectiveFrom.AddDays(-1),
            null,
            Now.AddHours(1));
        var afterResult = schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            EffectiveUntil.AddDays(1),
            null,
            Now.AddHours(2));

        Assert.Equal(RouteScheduleErrors.AssignmentOutsideSchedulePeriod, beforeResult.Error);
        Assert.Equal(RouteScheduleErrors.AssignmentOutsideSchedulePeriod, afterResult.Error);
        Assert.Empty(schedule.Assignments);
    }

    [Fact]
    public void InactiveScheduleShouldNotReceiveAssignments()
    {
        var schedule = CreateSchedule().Value;
        schedule.Deactivate(Now.AddHours(1));

        var result = schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            EffectiveFrom,
            null,
            Now.AddHours(2));

        Assert.Equal(RouteScheduleErrors.InactiveScheduleCannotAssign, result.Error);
    }

    [Fact]
    public void EndCurrentAssignmentShouldCloseOpenValidity()
    {
        var schedule = CreateSchedule().Value;
        schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            EffectiveFrom,
            null,
            Now.AddHours(1));

        var result = schedule.EndCurrentAssignment(
            new DateOnly(2026, 9, 20),
            Now.AddHours(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new DateOnly(2026, 9, 20),
            schedule.Assignments.Single().ValidUntil);
    }

    [Fact]
    public void ConfigureRecurrenceShouldRequireClosingOpenAssignmentBeforeSettingEndDate()
    {
        var schedule = CreateSchedule().Value;
        schedule.AssignResources(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            EffectiveFrom,
            null,
            Now.AddHours(1));

        var rejectedResult = schedule.ConfigureRecurrence(
            schedule.Shift,
            schedule.StartTime,
            schedule.EndTime,
            schedule.Days.Select(day => day.DayOfWeek),
            schedule.EffectiveFrom,
            new DateOnly(2026, 9, 20),
            schedule.DefaultAmount,
            schedule.DefaultCurrency,
            Now.AddHours(2));

        Assert.Equal(
            RouteScheduleErrors.OpenAssignmentMustBeClosed,
            rejectedResult.Error);
        Assert.Equal(EffectiveUntil, schedule.EffectiveUntil);

        schedule.EndCurrentAssignment(
            new DateOnly(2026, 9, 15),
            Now.AddHours(3));

        var acceptedResult = schedule.ConfigureRecurrence(
            schedule.Shift,
            schedule.StartTime,
            schedule.EndTime,
            schedule.Days.Select(day => day.DayOfWeek),
            schedule.EffectiveFrom,
            new DateOnly(2026, 9, 20),
            schedule.DefaultAmount,
            schedule.DefaultCurrency,
            Now.AddHours(4));

        Assert.True(acceptedResult.IsSuccess);
        Assert.Equal(new DateOnly(2026, 9, 20), schedule.EffectiveUntil);
    }

    private static Result<RouteSchedule> CreateSchedule(
        Guid? id = null,
        Guid? companyId = null,
        Guid? routeId = null,
        IEnumerable<DayOfWeek>? days = null) =>
        RouteSchedule.Create(
            id ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            routeId ?? Guid.NewGuid(),
            RouteShift.Morning,
            new TimeOnly(6, 30),
            new TimeOnly(7, 15),
            days ??
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            ],
            EffectiveFrom,
            EffectiveUntil,
            150m,
            " bob ",
            Now);
}
