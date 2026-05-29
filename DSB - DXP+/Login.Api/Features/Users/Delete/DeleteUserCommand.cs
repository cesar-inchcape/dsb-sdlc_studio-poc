using MediatR;

namespace Login.Api.Features.Users.Delete;

public class DeleteUserCommand : IRequest<DeleteUserResponse>
{
    public required Guid UserId { get; set; }
    public required Guid RequesterId { get; set; }
    public required List<string> RequesterRoles { get; set; }
}
