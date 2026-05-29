using MediatR;

namespace Login.Api.Features.Advisors.Delete;

public class DeleteAdvisorCommand : IRequest<DeleteAdvisorResponse>
{
    public Guid AdvisorId { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class DeleteAdvisorResponse
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
}
