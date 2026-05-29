namespace Login.Api.Features.Users.AssignRole;

public class AssignRoleResponse
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string Message { get; set; } = string.Empty;
}
