using System.Security.Claims;
using MediatR;
using Login.Api.Features.Users.Create;
using Login.Api.Features.Users.Update;
using Login.Api.Features.Users.Get;
using Login.Api.Features.Users.Delete;
using Login.Api.Features.Users.AssignRole;
using Login.Api.Features.Users.RemoveRole;
using Login.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Users");

        // Admin endpoints - SuperAdminOnly
        var adminGroup = group.MapGroup("/admin/users")
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
            .WithOpenApi();

        adminGroup.MapPost("", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user (SuperAdmin only)")
            .WithDescription("Creates a new user with the specified email, name, and password");

        adminGroup.MapGet("", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get paginated list of users (SuperAdmin only)")
            .WithDescription("Retrieves a paginated list of active users with optional filtering");

        adminGroup.MapGet("{id}", GetUser)
            .WithName("GetUser")
            .WithSummary("Get user by ID (SuperAdmin only)")
            .WithDescription("Retrieves a specific user by their ID");

        adminGroup.MapPut("{id}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update user (SuperAdmin only)")
            .WithDescription("Updates user information");

        adminGroup.MapDelete("{id}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Deactivate user (SuperAdmin only)")
            .WithDescription("Deactivates a user by ID (soft delete)");

        adminGroup.MapPost("{id}/roles/{roleId}", AssignRole)
            .WithName("AssignRole")
            .WithSummary("Assign role to user (SuperAdmin only)")
            .WithDescription("Assigns a role to a user");

        adminGroup.MapDelete("{id}/roles/{roleId}", RemoveRole)
            .WithName("RemoveRole")
            .WithSummary("Remove role from user (SuperAdmin only)")
            .WithDescription("Removes a role from a user");

        // Management endpoints - AdminOrHigher
        var mgmtGroup = group.MapGroup("/management/users")
            .RequireAuthorization(AuthorizationPolicies.AdminOrHigher)
            .WithOpenApi();

        mgmtGroup.MapGet("me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user profile")
            .WithDescription("Retrieves the current authenticated user's profile");

        mgmtGroup.MapPut("me", UpdateCurrentUser)
            .WithName("UpdateCurrentUser")
            .WithSummary("Update current user profile")
            .WithDescription("Updates the current authenticated user's profile");
    }

    // Admin Endpoints
    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserCommand command,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/admin/users/{result.Id}", result);
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

    private static async Task<IResult> GetUsers(
        [FromServices] IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? emailFilter = null,
        [FromQuery] string? firstNameFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            EmailFilter = emailFilter,
            FirstNameFilter = firstNameFilter
        };

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUser(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserQuery { UserId = id };
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return Results.NotFound($"User {id} not found");
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new UpdateUserCommand
            {
                UserId = id,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                RequesterId = userId,
                RequesterRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteUser(
        [FromRoute] Guid id,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new DeleteUserCommand
            {
                UserId = id,
                RequesterId = userId,
                RequesterRoles = roles
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

    private static async Task<IResult> AssignRole(
        [FromRoute] Guid id,
        [FromRoute] Guid roleId,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new AssignRoleCommand
            {
                UserId = id,
                RoleId = roleId,
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
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> RemoveRole(
        [FromRoute] Guid id,
        [FromRoute] Guid roleId,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new RemoveRoleCommand
            {
                UserId = id,
                RoleId = roleId,
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
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // Management Endpoints
    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var query = new GetUserQuery { UserId = userId };
            var result = await mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return Results.NotFound("Current user not found");
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.StatusCode(500);
        }
    }

    private static async Task<IResult> UpdateCurrentUser(
        [FromBody] UpdateUserRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new UpdateUserCommand
            {
                UserId = userId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                RequesterId = userId,
                RequesterRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
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
public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
}
