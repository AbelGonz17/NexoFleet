using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Infrastructure.Persistence;

namespace NexoFleet.Infrastructure.UnitTests.Persistence;

public sealed class RouteScheduleConfigurationTests
{
    [Fact]
    public void RouteScheduleShouldHaveTheExpectedDatabaseConfiguration()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=nexofleet;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);

        var scheduleEntity = context.Model.FindEntityType(typeof(RouteSchedule));

        Assert.NotNull(scheduleEntity);
        Assert.Equal("route_schedules", scheduleEntity.GetTableName());
        Assert.Equal(
            typeof(string),
            scheduleEntity.FindProperty(nameof(RouteSchedule.Shift))?.GetProviderClrType());
        Assert.Equal(
            typeof(string),
            scheduleEntity.FindProperty(nameof(RouteSchedule.Status))?.GetProviderClrType());
        Assert.Equal(
            18,
            scheduleEntity.FindProperty(nameof(RouteSchedule.DefaultAmount))?.GetPrecision());
        Assert.Equal(
            2,
            scheduleEntity.FindProperty(nameof(RouteSchedule.DefaultAmount))?.GetScale());

        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        var designScheduleEntity = designTimeModel.FindEntityType(typeof(RouteSchedule));
        Assert.NotNull(designScheduleEntity);
        Assert.Contains(
            designScheduleEntity.GetCheckConstraints(),
            constraint => constraint.Name == "ck_route_schedules_effective_period");

        var routeForeignKey = scheduleEntity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Route));
        Assert.Equal(DeleteBehavior.Restrict, routeForeignKey.DeleteBehavior);
        Assert.Collection(
            routeForeignKey.Properties,
            property => Assert.Equal(nameof(RouteSchedule.CompanyId), property.Name),
            property => Assert.Equal(nameof(RouteSchedule.RouteId), property.Name));

        var dayEntity = context.Model.FindEntityType(typeof(RouteScheduleDay));
        Assert.NotNull(dayEntity);
        Assert.Equal("route_schedule_days", dayEntity.GetTableName());
        Assert.Equal(2, dayEntity.FindPrimaryKey()?.Properties.Count);
        Assert.Equal(
            typeof(string),
            dayEntity.FindProperty(nameof(RouteScheduleDay.DayOfWeek))?.GetProviderClrType());

        var assignmentEntity = context.Model.FindEntityType(typeof(RouteScheduleAssignment));
        Assert.NotNull(assignmentEntity);
        Assert.Equal("route_schedule_assignments", assignmentEntity.GetTableName());
        var designAssignmentEntity = designTimeModel.FindEntityType(
            typeof(RouteScheduleAssignment));
        Assert.NotNull(designAssignmentEntity);
        Assert.Contains(
            designAssignmentEntity.GetCheckConstraints(),
            constraint => constraint.Name == "ck_route_schedule_assignments_valid_period");

        var openAssignmentIndex = assignmentEntity.GetIndexes().Single(index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(RouteScheduleAssignment.RouteScheduleId));
        Assert.True(openAssignmentIndex.IsUnique);
        Assert.Equal("\"valid_until\" IS NULL", openAssignmentIndex.GetFilter());

        var scheduleForeignKey = assignmentEntity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(RouteSchedule));
        Assert.Equal(DeleteBehavior.Cascade, scheduleForeignKey.DeleteBehavior);

        var employeeForeignKey = assignmentEntity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Employee));
        Assert.Equal(DeleteBehavior.Restrict, employeeForeignKey.DeleteBehavior);
        Assert.Collection(
            employeeForeignKey.Properties,
            property => Assert.Equal(nameof(RouteScheduleAssignment.CompanyId), property.Name),
            property => Assert.Equal(nameof(RouteScheduleAssignment.EmployeeId), property.Name));

        var vehicleForeignKey = assignmentEntity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Vehicle));
        Assert.Equal(DeleteBehavior.Restrict, vehicleForeignKey.DeleteBehavior);
        Assert.Collection(
            vehicleForeignKey.Properties,
            property => Assert.Equal(nameof(RouteScheduleAssignment.CompanyId), property.Name),
            property => Assert.Equal(nameof(RouteScheduleAssignment.VehicleId), property.Name));
    }
}
