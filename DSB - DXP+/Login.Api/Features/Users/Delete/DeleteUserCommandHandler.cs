using MediatR;
using Login.Api.Infrastructure.Data;

namespace Login.Api.Features.Users.Delete;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserResponse>
{
    private readonly LoginDbContext _context;

    public DeleteUserCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteUserResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var user = _context.Users.FirstOrDefault(u => u.Id == request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Authorization: Only SuperAdmin can delete
        if (!request.RequesterRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can deactivate users");
        }

        // Deactivate user
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteUserResponse
        {
            UserId = user.Id,
            Success = true
        };
    }
}
