using MediatR;
using Login.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Users.Get;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly LoginDbContext _context;

    public GetUserQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
