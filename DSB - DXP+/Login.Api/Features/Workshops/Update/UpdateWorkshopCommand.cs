using MediatR;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Workshops.Update;

public class UpdateWorkshopCommand : IRequest<UpdateWorkshopResponse>
{
    public Guid WorkshopId { get; set; }
    public string? Name { get; set; }
    public int? Capacity { get; set; }
    public UpdateWorkshopAddress? Address { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class UpdateWorkshopAddress
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

public class UpdateWorkshopResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Brand { get; set; }
    public required string Location { get; set; }
    public int Capacity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
