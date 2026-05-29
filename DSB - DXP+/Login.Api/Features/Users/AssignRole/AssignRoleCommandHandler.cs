using MediatR;
using Login.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Users.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, AssignRoleResponse>
{
    private readonly LoginDbContext _context;

    public AssignRoleCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<AssignRoleResponse> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can assign roles");
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

        // Check if user already has this role
        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);
        
        if (existingUserRole != null)
        {
            throw new InvalidOperationException($"User {request.UserId} already has role {request.RoleId}");
        }

        // Create new user-role assignment
        var userRole = new Infrastructure.Data.Entities.UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return new AssignRoleResponse
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            Message = $"Role {role.Name} assigned to user {user.Email}"
        };
    }
}
