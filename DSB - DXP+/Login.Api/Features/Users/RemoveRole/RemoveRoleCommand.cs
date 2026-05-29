using MediatR;

namespace Login.Api.Features.Users.RemoveRole;

public class RemoveRoleCommand : IRequest<RemoveRoleResponse>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}
