using MediatR;
using Login.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Advisors.Get;

public class GetAdvisorQueryHandler : IRequestHandler<GetAdvisorQuery, AdvisorDto?>
{
    private readonly LoginDbContext _context;

    public GetAdvisorQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<AdvisorDto?> Handle(GetAdvisorQuery request, CancellationToken cancellationToken)
    {
        var advisor = await _context.Advisors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AdvisorId && a.IsActive, cancellationToken);

        if (advisor == null)
            return null;

        return new AdvisorDto
        {
            Id = advisor.Id,
            FirstName = advisor.FirstName,
            LastName = advisor.LastName,
            Email = advisor.Email,
            PhoneNumber = advisor.PhoneNumber,
            WorkshopId = advisor.WorkshopId,
            AssignedBrand = advisor.AssignedBrand.ToString(),
            AvailableHoursPerDay = advisor.AvailableHoursPerDay,
            IsActive = advisor.IsActive,
            CreatedAt = advisor.CreatedAt,
            UpdatedAt = advisor.UpdatedAt
        };
    }
}

public class GetAdvisorsQueryHandler : IRequestHandler<GetAdvisorsQuery, GetAdvisorsResponse>
{
    private readonly LoginDbContext _context;

    public GetAdvisorsQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<GetAdvisorsResponse> Handle(GetAdvisorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Advisors.AsNoTracking().Where(a => a.IsActive);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.EmailFilter))
        {
            var emailLower = request.EmailFilter.ToLower();
            query = query.Where(a => a.Email.ToLower().Contains(emailLower));
        }

        if (request.WorkshopIdFilter.HasValue)
        {
            query = query.Where(a => a.WorkshopId == request.WorkshopIdFilter.Value);
        }

        if (request.BrandFilter.HasValue)
        {
            query = query.Where(a => a.AssignedBrand == request.BrandFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var advisors = await query
            .OrderBy(a => a.Email)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetAdvisorsResponse
        {
            Advisors = advisors.Select(a => new AdvisorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                WorkshopId = a.WorkshopId,
                AssignedBrand = a.AssignedBrand.ToString(),
                AvailableHoursPerDay = a.AvailableHoursPerDay,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }
}
