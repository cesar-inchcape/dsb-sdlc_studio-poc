using MediatR;

namespace Login.Api.Features.Users.AssignRole;

public class AssignRoleCommand : IRequest<AssignRoleResponse>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}
