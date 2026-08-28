using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity;
using NexoFleet.Domain.Auditing;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Notifications;
using NexoFleet.Domain.Payments;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Infrastructure.Persistence;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.UnitTests.Persistence;

public sealed class RemainingEntitiesConfigurationTests
{
    [Fact]
    public void ModelShouldUseSnakeCaseForDomainAndIdentityObjects()
    {
        using var context = CreateContext();
        var user = context.Model.FindEntityType(typeof(ApplicationUser));
        var role = context.Model.FindEntityType(typeof(IdentityRole<Guid>));
        var trip = context.Model.FindEntityType(typeof(Trip));

        Assert.NotNull(user);
        Assert.NotNull(role);
        Assert.NotNull(trip);
        Assert.Equal("asp_net_users", user.GetTableName());
        Assert.Equal("asp_net_roles", role.GetTableName());
        Assert.Equal("company_id", user.FindProperty(nameof(ApplicationUser.CompanyId))?.GetColumnName());
        Assert.Equal("trip_number", trip.FindProperty(nameof(Trip.TripNumber))?.GetColumnName());
        Assert.All(user.GetIndexes(), index => Assert.Equal(index.GetDatabaseName()?.ToLowerInvariant(), index.GetDatabaseName()));
    }

    [Fact]
    public void AuditLogShouldUseJsonAndAuditIndexes()
    {
        using var context = CreateContext();
        var auditLog = context.Model.FindEntityType(typeof(AuditLog));

        Assert.NotNull(auditLog);
        Assert.Equal("audit_logs", auditLog.GetTableName());
        Assert.Equal("jsonb", auditLog.FindProperty(nameof(AuditLog.Data))?.GetColumnType());
        Assert.Contains(auditLog.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([
                nameof(AuditLog.CompanyId), nameof(AuditLog.OccurredAtUtc)]));
    }

    [Fact]
    public void VehicleDocumentShouldBeOwnedByVehicleAggregate()
    {
        using var context = CreateContext();
        var document = context.Model.FindEntityType(typeof(VehicleDocument));

        Assert.NotNull(document);
        Assert.Equal("vehicle_documents", document.GetTableName());
        var vehicleForeignKey = document.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Vehicle));
        Assert.Equal(DeleteBehavior.Cascade, vehicleForeignKey.DeleteBehavior);
    }

    [Fact]
    public void ClientShouldHaveTenantScopedUniqueCode()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(Client));

        Assert.NotNull(entity);
        Assert.Equal("clients", entity.GetTableName());
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Client.CompanyId), nameof(Client.ClientCode)]));
    }

    [Fact]
    public void TripShouldMapCompleteAggregate()
    {
        using var context = CreateContext();
        var trip = context.Model.FindEntityType(typeof(Trip));

        Assert.NotNull(trip);
        Assert.Equal("trips", trip.GetTableName());
        Assert.Null(trip.FindNavigation(nameof(Trip.CurrentAssignment)));
        Assert.Equal("trip_assignments", context.Model.FindEntityType(typeof(TripAssignment))?.GetTableName());
        Assert.Equal("trip_status_history", context.Model.FindEntityType(typeof(TripStatusHistory))?.GetTableName());
        Assert.Equal("trip_reviews", context.Model.FindEntityType(typeof(TripReview))?.GetTableName());
        Assert.Equal("trip_incidents", context.Model.FindEntityType(typeof(TripIncident))?.GetTableName());
        Assert.Equal("trip_files", context.Model.FindEntityType(typeof(TripFile))?.GetTableName());
    }

    [Fact]
    public void PaymentPeriodShouldHaveDateConstraint()
    {
        using var context = CreateContext();
        var period = context.Model.FindEntityType(typeof(PaymentPeriod));
        var designPeriod = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(PaymentPeriod));

        Assert.NotNull(period);
        Assert.NotNull(designPeriod);
        Assert.Equal("payment_periods", period.GetTableName());
        Assert.Contains(designPeriod.GetCheckConstraints(), constraint => constraint.Name == "ck_payment_periods_dates");
    }

    [Fact]
    public void PaymentReportShouldMapItemsCommentsAndFiles()
    {
        using var context = CreateContext();
        var report = context.Model.FindEntityType(typeof(PaymentReport));

        Assert.NotNull(report);
        Assert.Equal("payment_reports", report.GetTableName());
        Assert.Null(report.FindProperty(nameof(PaymentReport.TotalAmount)));
        Assert.Equal("payment_items", context.Model.FindEntityType(typeof(PaymentItem))?.GetTableName());
        Assert.Equal("payment_comments", context.Model.FindEntityType(typeof(PaymentComment))?.GetTableName());
        Assert.Equal("payment_report_files", context.Model.FindEntityType(typeof(PaymentReportFile))?.GetTableName());
        Assert.Contains(report.GetIndexes(), index => index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual([
                nameof(PaymentReport.CompanyId), nameof(PaymentReport.PaymentPeriodId), nameof(PaymentReport.EmployeeId)]));
    }

    [Fact]
    public void NotificationShouldBeIndexedForRecipientInbox()
    {
        using var context = CreateContext();
        var notification = context.Model.FindEntityType(typeof(Notification));

        Assert.NotNull(notification);
        Assert.Equal("notifications", notification.GetTableName());
        Assert.Contains(notification.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([
                nameof(Notification.RecipientUserId), nameof(Notification.Status), nameof(Notification.CreatedAtUtc)]));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=nexofleet;Username=test;Password=test")
            .Options;
        return new ApplicationDbContext(options);
    }
}
