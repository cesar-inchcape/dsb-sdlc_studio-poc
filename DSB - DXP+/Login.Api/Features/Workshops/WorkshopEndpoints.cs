using System.Security.Claims;
using MediatR;
using Login.Api.Features.Workshops.Create;
using Login.Api.Features.Workshops.Get;
using Login.Api.Features.Workshops.Update;
using Login.Api.Features.Workshops.Delete;
using Login.Api.Infrastructure.Authorization;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Api.Features.Workshops;

public static class WorkshopEndpoints
{
    public static void MapWorkshopEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Workshops");

        // Admin endpoints - SuperAdminOnly
        var adminGroup = group.MapGroup("/admin/workshops")
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
            .WithOpenApi();

        adminGroup.MapPost("", CreateWorkshop)
            .WithName("CreateWorkshop")
            .WithSummary("Create a new workshop (SuperAdmin only)")
            .WithDescription("Creates a new workshop with initial schedule configuration");

        adminGroup.MapGet("", GetWorkshops)
            .WithName("GetWorkshops")
            .WithSummary("Get paginated list of workshops (SuperAdmin only)")
            .WithDescription("Retrieves workshops with optional brand and location filtering");

        adminGroup.MapGet("{id}", GetWorkshop)
            .WithName("GetWorkshop")
            .WithSummary("Get workshop by ID (SuperAdmin only)")
            .WithDescription("Retrieves a specific workshop by its ID");

        adminGroup.MapPut("{id}", UpdateWorkshop)
            .WithName("UpdateWorkshop")
            .WithSummary("Update workshop (SuperAdmin only)")
            .WithDescription("Updates workshop information");

        adminGroup.MapDelete("{id}", DeleteWorkshop)
            .WithName("DeleteWorkshop")
            .WithSummary("Deactivate workshop (SuperAdmin only)")
            .WithDescription("Deactivates a workshop by ID (soft delete)");

        // Management endpoints - AdminOrHigher (scoped to assigned brands)
        var mgmtGroup = group.MapGroup("/management/workshops")
            .RequireAuthorization(AuthorizationPolicies.AdminOrHigher)
            .WithOpenApi();

        mgmtGroup.MapGet("", GetManagementWorkshops)
            .WithName("GetManagementWorkshops")
            .WithSummary("Get workshops for assigned brands")
            .WithDescription("Retrieves workshops accessible to the current user's assigned brands");
    }

    // Admin Endpoints
    private static async Task<IResult> CreateWorkshop(
        [FromBody] CreateWorkshopRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new CreateWorkshopCommand
            {
                Name = request.Name,
                Brand = request.Brand,
                Location = request.Location,
                Address = new CreateWorkshopAddress
                {
                    Street = request.Address.Street,
                    City = request.Address.City,
                    Region = request.Address.Region,
                    PostalCode = request.Address.PostalCode,
                    Country = request.Address.Country ?? "Chile"
                },
                Capacity = request.Capacity,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/admin/workshops/{result.Id}", result);
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

    private static async Task<IResult> GetWorkshops(
        [FromServices] IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] WorkshopBrand? brand = null,
        [FromQuery] string? location = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWorkshopsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            BrandFilter = brand,
            LocationFilter = location
        };

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetWorkshop(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkshopQuery { WorkshopId = id };
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return Results.NotFound($"Workshop {id} not found");
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateWorkshop(
        [FromRoute] Guid id,
        [FromBody] UpdateWorkshopRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new UpdateWorkshopCommand
            {
                WorkshopId = id,
                Name = request.Name,
                Capacity = request.Capacity,
                Address = request.Address != null ? new UpdateWorkshopAddress
                {
                    Street = request.Address.Street,
                    City = request.Address.City,
                    Region = request.Address.Region,
                    PostalCode = request.Address.PostalCode,
                    Country = request.Address.Country
                } : null,
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

    private static async Task<IResult> DeleteWorkshop(
        [FromRoute] Guid id,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new DeleteWorkshopCommand
            {
                WorkshopId = id,
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
    private static async Task<IResult> GetManagementWorkshops(
        [FromServices] IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] WorkshopBrand? brand = null,
        [FromQuery] string? location = null,
        CancellationToken cancellationToken = default)
    {
        // For now, same as admin. Future: filter by user's assigned brands
        var query = new GetWorkshopsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            BrandFilter = brand,
            LocationFilter = location
        };

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}

// Request DTOs
public class CreateWorkshopRequest
{
    public required string Name { get; set; }
    public WorkshopBrand Brand { get; set; }
    public required string Location { get; set; }
    public required WorkshopAddressRequest Address { get; set; }
    public int Capacity { get; set; }
}

public class UpdateWorkshopRequest
{
    public string? Name { get; set; }
    public int? Capacity { get; set; }
    public WorkshopAddressRequest? Address { get; set; }
}

public class WorkshopAddressRequest
{
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string Region { get; set; }
    public required string PostalCode { get; set; }
    public string Country { get; set; } = "Chile";
}
