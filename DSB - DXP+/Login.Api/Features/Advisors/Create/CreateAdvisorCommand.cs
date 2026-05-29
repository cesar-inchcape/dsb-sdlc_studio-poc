using MediatR;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Advisors.Create;

public class CreateAdvisorCommand : IRequest<CreateAdvisorResponse>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public Guid WorkshopId { get; set; }
    public AdvisorBrand AssignedBrand { get; set; }
    public int AvailableHoursPerDay { get; set; } = 8;
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class CreateAdvisorResponse
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public Guid WorkshopId { get; set; }
    public required string AssignedBrand { get; set; }
    public int AvailableHoursPerDay { get; set; }
    public DateTime CreatedAt { get; set; }
}
