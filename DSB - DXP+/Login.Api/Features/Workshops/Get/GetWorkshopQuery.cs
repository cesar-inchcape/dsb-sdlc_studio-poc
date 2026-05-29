using MediatR;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Workshops.Get;

public class GetWorkshopQuery : IRequest<WorkshopDto?>
{
    public Guid WorkshopId { get; set; }
}

public class GetWorkshopsQuery : IRequest<GetWorkshopsResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public WorkshopBrand? BrandFilter { get; set; }
    public string? LocationFilter { get; set; }
    public Guid? RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class WorkshopDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Brand { get; set; }
    public required string Location { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetWorkshopsResponse
{
    public List<WorkshopDto> Workshops { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
