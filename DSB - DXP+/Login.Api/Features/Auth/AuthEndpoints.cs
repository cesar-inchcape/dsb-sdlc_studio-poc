using Login.Api.Features.Auth.Login;
using Login.Api.Features.Auth.RefreshToken;
using Login.Api.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new LoginCommand(request.Email, request.Password);
                var response = await mediator.Send(command, cancellationToken);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("Login")
        .WithSummary("Authenticate user and obtain JWT tokens")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new RefreshTokenCommand(request.RefreshToken);
                var response = await mediator.Send(command, cancellationToken);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh access token using refresh token")
        .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // Protected endpoints for authorization testing
        var adminGroup = app.MapGroup("/api/admin")
            .WithTags("Administration")
            .WithOpenApi()
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly);

        adminGroup.MapGet("/users", () =>
        {
            return Results.Ok(new { message = "SuperAdmin users data" });
        })
        .WithName("GetAllUsers")
        .WithSummary("Get all users (SuperAdmin only)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        var readOnlyGroup = app.MapGroup("/api/workshop")
            .WithTags("Workshop")
            .WithOpenApi()
            .RequireAuthorization(AuthorizationPolicies.WorkshopReadOnly);

        readOnlyGroup.MapGet("/info", (HttpContext context) =>
        {
            var userEmail = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";
            return Results.Ok(new { message = "Workshop read-only info", userEmail });
        })
        .WithName("GetWorkshopInfo")
        .WithSummary("Get workshop info (authenticated users)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
