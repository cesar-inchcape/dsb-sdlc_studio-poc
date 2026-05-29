using System.Security.Claims;
using MediatR;
using Login.Api.Features.Advisors.Create;
using Login.Api.Features.Advisors.Get;
using Login.Api.Features.Advisors.Update;
using Login.Api.Features.Advisors.Delete;
using Login.Api.Infrastructure.Authorization;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Api.Features.Advisors;

public static class AdvisorEndpoints
{
    public static void MapAdvisorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Advisors");

        // Admin endpoints - SuperAdminOnly
        var adminGroup = group.MapGroup("/admin/advisors")
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
            .WithOpenApi();

        adminGroup.MapPost("", CreateAdvisor)
            .WithName("CreateAdvisor")
            .WithSummary("Create a new advisor (SuperAdmin only)")
            .WithDescription("Creates a new advisor assigned to a workshop and brand");

        adminGroup.MapGet("", GetAdvisors)
            .WithName("GetAdvisors")
            .WithSummary("Get paginated list of advisors (SuperAdmin only)")
            .WithDescription("Retrieves advisors with optional email, workshop, and brand filtering");

        adminGroup.MapGet("{id}", GetAdvisor)
            .WithName("GetAdvisor")
            .WithSummary("Get advisor by ID (SuperAdmin only)")
            .WithDescription("Retrieves a specific advisor by ID");

        adminGroup.MapPut("{id}", UpdateAdvisor)
            .WithName("UpdateAdvisor")
            .WithSummary("Update advisor (SuperAdmin only)")
            .WithDescription("Updates advisor information");

        adminGroup.MapDelete("{id}", DeleteAdvisor)
            .WithName("DeleteAdvisor")
            .WithSummary("Deactivate advisor (SuperAdmin only)")
            .WithDescription("Deactivates an advisor by ID (soft delete)");

        // Management endpoints - AdminOrHigher (scoped read access)
        var mgmtGroup = group.MapGroup("/management/advisors")
            .RequireAuthorization(AuthorizationPolicies.AdminOrHigher)
            .WithOpenApi();

        mgmtGroup.MapGet("", GetManagementAdvisors)
            .WithName("GetManagementAdvisors")
            .WithSummary("Get advisors for assigned workshop")
            .WithDescription("Retrieves advisors accessible to the current user's assigned workshop");
    }

    // Admin Endpoints
    private static async Task<IResult> CreateAdvisor(
        [FromBody] CreateAdvisorRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new CreateAdvisorCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                WorkshopId = request.WorkshopId,
                AssignedBrand = request.AssignedBrand,
                AvailableHoursPerDay = request.AvailableHoursPerDay,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/admin/advisors/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetAdvisors(
        [FromServices] IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? email = null,
        [FromQuery] Guid? workshopId = null,
        [FromQuery] AdvisorBrand? brand = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdvisorsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            EmailFilter = email,
            WorkshopIdFilter = workshopId,
            BrandFilter = brand
        };

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAdvisor(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAdvisorQuery { AdvisorId = id };
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return Results.NotFound($"Advisor {id} not found");
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateAdvisor(
        [FromRoute] Guid id,
        [FromBody] UpdateAdvisorRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new UpdateAdvisorCommand
            {
                AdvisorId = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                AssignedBrand = request.AssignedBrand,
                AvailableHoursPerDay = request.AvailableHoursPerDay,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
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

    private static async Task<IResult> DeleteAdvisor(
        [FromRoute] Guid id,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new DeleteAdvisorCommand
            {
                AdvisorId = id,
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

    // Management Endpoints
    private static async Task<IResult> GetManagementAdvisors(
        [FromServices] IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? email = null,
        [FromQuery] AdvisorBrand? brand = null,
        CancellationToken cancellationToken = default)
    {
        // For now, same as admin. Future: filter by user's assigned workshop
        var query = new GetAdvisorsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            EmailFilter = email,
            BrandFilter = brand
        };

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}

// Request DTOs
public class CreateAdvisorRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public Guid WorkshopId { get; set; }
    public AdvisorBrand AssignedBrand { get; set; }
    public int AvailableHoursPerDay { get; set; } = 8;
}

public class UpdateAdvisorRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public AdvisorBrand? AssignedBrand { get; set; }
    public int? AvailableHoursPerDay { get; set; }
}
