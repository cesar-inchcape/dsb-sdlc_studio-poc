using MediatR;
using Login.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Users.Get;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, GetUsersResponse>
{
    private readonly LoginDbContext _context;

    public GetUsersQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<GetUsersResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Start with active users query
        var query = _context.Users
            .Where(u => u.IsActive)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.EmailFilter))
        {
            query = query.Where(u => u.Email.ToLower().Contains(request.EmailFilter.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.FirstNameFilter))
        {
            query = query.Where(u => u.FirstName.ToLower().Contains(request.FirstNameFilter.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.LastNameFilter))
        {
            query = query.Where(u => u.LastName.ToLower().Contains(request.LastNameFilter.ToLower()));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();

        return new GetUsersResponse
        {
            Users = userDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
