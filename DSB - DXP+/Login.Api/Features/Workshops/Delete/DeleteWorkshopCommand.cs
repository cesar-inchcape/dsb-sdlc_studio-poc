using MediatR;

namespace Login.Api.Features.Workshops.Delete;

public class DeleteWorkshopCommand : IRequest<DeleteWorkshopResponse>
{
    public Guid WorkshopId { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class DeleteWorkshopResponse
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
}
