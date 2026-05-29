using MediatR;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Advisors.Get;

public class GetAdvisorQuery : IRequest<AdvisorDto?>
{
    public Guid AdvisorId { get; set; }
}

public class GetAdvisorsQuery : IRequest<GetAdvisorsResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? EmailFilter { get; set; }
    public Guid? WorkshopIdFilter { get; set; }
    public AdvisorBrand? BrandFilter { get; set; }
}

public class GetAdvisorsResponse
{
    public required List<AdvisorDto> Advisors { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AdvisorDto
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public Guid WorkshopId { get; set; }
    public required string AssignedBrand { get; set; }
    public int AvailableHoursPerDay { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
