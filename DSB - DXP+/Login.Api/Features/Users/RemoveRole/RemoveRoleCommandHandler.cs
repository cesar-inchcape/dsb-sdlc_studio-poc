using MediatR;
using Login.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Users.RemoveRole;

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, RemoveRoleResponse>
{
    private readonly LoginDbContext _context;

    public RemoveRoleCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<RemoveRoleResponse> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can remove roles");
        }

        // Validate user exists
        var user = await _context.Users.FindAsync(new object?[] { request.UserId }, cancellationToken: cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        // Validate role exists
        var role = await _context.Roles.FindAsync(new object?[] { request.RoleId }, cancellationToken: cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException($"Role {request.RoleId} not found");
        }

        // Find and remove the user-role assignment
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);
        
        if (userRole == null)
        {
            throw new InvalidOperationException($"User {request.UserId} does not have role {request.RoleId}");
        }

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return new RemoveRoleResponse
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            Message = $"Role {role.Name} removed from user {user.Email}"
        };
    }
}
