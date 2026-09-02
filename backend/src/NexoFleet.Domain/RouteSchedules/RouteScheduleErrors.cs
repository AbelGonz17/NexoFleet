using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.RouteSchedules;

public static class RouteScheduleErrors
{
    public const int CurrencyLength = 3;

    public static readonly Error InvalidId = Error.Validation(
        "RouteSchedule.InvalidId",
        "El identificador de la programación no es válido.");

    public static readonly Error InvalidCompanyId = Error.Validation(
        "RouteSchedule.InvalidCompanyId",
        "El identificador de la empresa no es válido.");

    public static readonly Error InvalidRouteId = Error.Validation(
        "RouteSchedule.InvalidRouteId",
        "El identificador de la ruta no es válido.");

    public static readonly Error InvalidShift = Error.Validation(
        "RouteSchedule.InvalidShift",
        "El turno de la programación no es válido.");

    public static readonly Error EndTimeEqualsStartTime = Error.Validation(
        "RouteSchedule.EndTimeEqualsStartTime",
        "La hora final no puede ser igual a la hora inicial.");

    public static readonly Error DaysRequired = Error.Validation(
        "RouteSchedule.DaysRequired",
        "Debe seleccionar al menos un día de ejecución.");

    public static readonly Error InvalidDayOfWeek = Error.Validation(
        "RouteSchedule.InvalidDayOfWeek",
        "La programación contiene un día de la semana no válido.");

    public static readonly Error InvalidEffectivePeriod = Error.Validation(
        "RouteSchedule.InvalidEffectivePeriod",
        "La fecha final de vigencia no puede ser anterior a la fecha inicial.");

    public static readonly Error InvalidDefaultAmount = Error.Validation(
        "RouteSchedule.InvalidDefaultAmount",
        "El monto predeterminado no puede ser negativo.");

    public static readonly Error DefaultAmountRequired = Error.Validation(
        "RouteSchedule.DefaultAmountRequired",
        "Debe indicar un monto cuando especifica una moneda.");

    public static readonly Error DefaultCurrencyRequired = Error.Validation(
        "RouteSchedule.DefaultCurrencyRequired",
        "Debe indicar la moneda del monto predeterminado.");

    public static readonly Error DefaultCurrencyInvalid = Error.Validation(
        "RouteSchedule.DefaultCurrencyInvalid",
        "La moneda debe utilizar un código de tres letras.");

    public static readonly Error AlreadyActive = Error.Conflict(
        "RouteSchedule.AlreadyActive",
        "La programación ya está activa.");

    public static readonly Error AlreadyInactive = Error.Conflict(
        "RouteSchedule.AlreadyInactive",
        "La programación ya está inactiva.");

    public static readonly Error InactiveScheduleCannotAssign = Error.Conflict(
        "RouteSchedule.InactiveScheduleCannotAssign",
        "Una programación inactiva no puede recibir nuevas asignaciones.");

    public static readonly Error InvalidAssignmentId = Error.Validation(
        "RouteSchedule.InvalidAssignmentId",
        "El identificador de la asignación no es válido.");

    public static readonly Error AssignmentAlreadyExists = Error.Conflict(
        "RouteSchedule.AssignmentAlreadyExists",
        "Ya existe una asignación con ese identificador.");

    public static readonly Error InvalidEmployeeId = Error.Validation(
        "RouteSchedule.InvalidEmployeeId",
        "El identificador del empleado no es válido.");

    public static readonly Error InvalidVehicleId = Error.Validation(
        "RouteSchedule.InvalidVehicleId",
        "El identificador del vehículo no es válido.");

    public static readonly Error InvalidAssignmentPeriod = Error.Validation(
        "RouteSchedule.InvalidAssignmentPeriod",
        "La fecha final de la asignación no puede ser anterior a la fecha inicial.");

    public static readonly Error AssignmentOutsideSchedulePeriod = Error.Validation(
        "RouteSchedule.AssignmentOutsideSchedulePeriod",
        "La asignación debe estar dentro de la vigencia de la programación.");

    public static readonly Error AssignmentPeriodOverlaps = Error.Conflict(
        "RouteSchedule.AssignmentPeriodOverlaps",
        "La vigencia de la asignación se superpone con otra asignación existente.");

    public static readonly Error AssignmentCannotRewriteHistory = Error.Conflict(
        "RouteSchedule.AssignmentCannotRewriteHistory",
        "La nueva asignación debe comenzar después de la asignación vigente.");

    public static readonly Error CurrentAssignmentNotFound = Error.NotFound(
        "RouteSchedule.CurrentAssignmentNotFound",
        "No existe una asignación vigente para finalizar.");

    public static readonly Error OpenAssignmentMustBeClosed = Error.Conflict(
        "RouteSchedule.OpenAssignmentMustBeClosed",
        "Debe cerrar la asignación vigente antes de limitar la fecha final de la programación.");

    public static readonly Error NotFound = Error.NotFound(
        "RouteSchedule.NotFound",
        "La programación de ruta no fue encontrada.");
}
