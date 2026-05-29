using MediatR;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Workshops.Get;

public class GetWorkshopQueryHandler : IRequestHandler<GetWorkshopQuery, WorkshopDto?>
{
    private readonly LoginDbContext _context;

    public GetWorkshopQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<WorkshopDto?> Handle(GetWorkshopQuery request, CancellationToken cancellationToken)
    {
        var workshop = await _context.Workshops
            .AsNoTracking()
            .Where(w => w.Id == request.WorkshopId && w.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (workshop == null)
            return null;

        return new WorkshopDto
        {
            Id = workshop.Id,
            Name = workshop.Name,
            Brand = workshop.Brand.ToString(),
            Location = workshop.Location,
            Capacity = workshop.Capacity,
            IsActive = workshop.IsActive,
            CreatedAt = workshop.CreatedAt,
            UpdatedAt = workshop.UpdatedAt
        };
    }
}

public class GetWorkshopsQueryHandler : IRequestHandler<GetWorkshopsQuery, GetWorkshopsResponse>
{
    private readonly LoginDbContext _context;

    public GetWorkshopsQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<GetWorkshopsResponse> Handle(GetWorkshopsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Workshops.AsNoTracking().Where(w => w.IsActive);

        // Apply brand filter
        if (request.BrandFilter.HasValue)
        {
            query = query.Where(w => w.Brand == request.BrandFilter.Value);
        }

        // Apply location filter (case-insensitive)
        if (!string.IsNullOrWhiteSpace(request.LocationFilter))
        {
            var locationLower = request.LocationFilter.ToLower();
            query = query.Where(w => w.Location.ToLower().Contains(locationLower));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var workshops = await query
            .OrderBy(w => w.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = workshops.Select(w => new WorkshopDto
        {
            Id = w.Id,
            Name = w.Name,
            Brand = w.Brand.ToString(),
            Location = w.Location,
            Capacity = w.Capacity,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt
        }).ToList();

        return new GetWorkshopsResponse
        {
            Workshops = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
