using MediatR;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Advisors.Update;

public class UpdateAdvisorCommand : IRequest<UpdateAdvisorResponse>
{
    public Guid AdvisorId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public AdvisorBrand? AssignedBrand { get; set; }
    public int? AvailableHoursPerDay { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class UpdateAdvisorResponse
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public Guid WorkshopId { get; set; }
    public required string AssignedBrand { get; set; }
    public int AvailableHoursPerDay { get; set; }
    public DateTime UpdatedAt { get; set; }
}
