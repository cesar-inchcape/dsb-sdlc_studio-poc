using System.Security.Claims;
using MediatR;
using Login.Api.Features.Schedules.WorkshopSchedule;
using Login.Api.Features.Schedules.Holiday;
using Login.Api.Features.Schedules.Blackout;
using Login.Api.Infrastructure.Authorization;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Api.Features.Schedules;

public static class ScheduleEndpoints
{
    public static void MapScheduleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Schedules");

        // Workshop Schedule endpoints
        var scheduleGroup = group.MapGroup("/admin/workshops/{workshopId}/schedule")
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
            .WithOpenApi();

        scheduleGroup.MapPut("{dayOfWeek}", UpdateWorkshopSchedule)
            .WithName("UpdateWorkshopSchedule")
            .WithSummary("Update workshop operating hours for a day")
            .WithDescription("Updates or creates a schedule entry for a specific day of week");

        scheduleGroup.MapGet("", GetWorkshopSchedule)
            .WithName("GetWorkshopSchedule")
            .WithSummary("Get workshop weekly schedule")
            .WithDescription("Retrieves the full weekly operating hours schedule");

        // Holiday endpoints
        var holidayGroup = group.MapGroup("/admin/workshops/{workshopId}/holidays")
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
            .WithOpenApi();

        holidayGroup.MapPost("", CreateHoliday)
            .WithName("CreateHoliday")
            .WithSummary("Create a workshop holiday")
            .WithDescription("Adds a single-day closure to the holiday calendar");

        holidayGroup.MapGet("", GetHolidays)
            .WithName("GetHolidays")
            .WithSummary("Get workshop holidays")
            .WithDescription("Retrieves holidays with optional date range filtering");

        holidayGroup.MapDelete("{holidayId}", DeleteHoliday)
            .WithName("DeleteHoliday")
            .WithSummary("Delete a workshop holiday")
            .WithDescription("Removes a holiday from the calendar");

        // Blackout date endpoints
        var blackoutGroup = group.MapGroup("/admin/workshops/{workshopId}/blackouts")
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
            .WithOpenApi();

        blackoutGroup.MapPost("", CreateBlackoutDate)
            .WithName("CreateBlackoutDate")
            .WithSummary("Create a workshop blackout period")
            .WithDescription("Adds a date-range closure (maintenance, training, etc.)");

        blackoutGroup.MapGet("", GetBlackoutDates)
            .WithName("GetBlackoutDates")
            .WithSummary("Get workshop blackout periods")
            .WithDescription("Retrieves blackout dates with optional date range filtering");

        blackoutGroup.MapDelete("{blackoutDateId}", DeleteBlackoutDate)
            .WithName("DeleteBlackoutDate")
            .WithSummary("Delete a workshop blackout period")
            .WithDescription("Removes a blackout date range");
    }

    // Workshop Schedule Handlers
    private static async Task<IResult> UpdateWorkshopSchedule(
        [FromRoute] Guid workshopId,
        [FromRoute] DayOfWeek dayOfWeek,
        [FromBody] UpdateScheduleRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new UpdateWorkshopScheduleCommand
            {
                WorkshopId = workshopId,
                DayOfWeek = dayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsOpen = request.IsOpen,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetWorkshopSchedule(
        [FromRoute] Guid workshopId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetWorkshopScheduleQuery { WorkshopId = workshopId };
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    // Holiday Handlers
    private static async Task<IResult> CreateHoliday(
        [FromRoute] Guid workshopId,
        [FromBody] CreateHolidayRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new CreateHolidayCommand
            {
                WorkshopId = workshopId,
                Date = request.Date,
                Reason = request.Reason,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/admin/workshops/{workshopId}/holidays/{result.Id}", result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetHolidays(
        [FromRoute] Guid workshopId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetHolidaysQuery
            {
                WorkshopId = workshopId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteHoliday(
        [FromRoute] Guid workshopId,
        [FromRoute] Guid holidayId,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new DeleteHolidayCommand
            {
                HolidayId = holidayId,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    // Blackout Date Handlers
    private static async Task<IResult> CreateBlackoutDate(
        [FromRoute] Guid workshopId,
        [FromBody] CreateBlackoutDateRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new CreateBlackoutDateCommand
            {
                WorkshopId = workshopId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/admin/workshops/{workshopId}/blackouts/{result.Id}", result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetBlackoutDates(
        [FromRoute] Guid workshopId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetBlackoutDatesQuery
            {
                WorkshopId = workshopId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteBlackoutDate(
        [FromRoute] Guid workshopId,
        [FromRoute] Guid blackoutDateId,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new DeleteBlackoutDateCommand
            {
                BlackoutDateId = blackoutDateId,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}

// Request DTOs
public class UpdateScheduleRequest
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsOpen { get; set; }
}

public class CreateHolidayRequest
{
    public DateTime Date { get; set; }
    public required string Reason { get; set; }
}

public class CreateBlackoutDateRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string Reason { get; set; }
}
