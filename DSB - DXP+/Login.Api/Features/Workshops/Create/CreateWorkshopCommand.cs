using MediatR;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Workshops.Create;

public class CreateWorkshopCommand : IRequest<CreateWorkshopResponse>
{
    public required string Name { get; set; }
    public WorkshopBrand Brand { get; set; }
    public required string Location { get; set; }
    public required CreateWorkshopAddress Address { get; set; }
    public int Capacity { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class CreateWorkshopAddress
{
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string Region { get; set; }
    public required string PostalCode { get; set; }
    public string Country { get; set; } = "Chile";
}
