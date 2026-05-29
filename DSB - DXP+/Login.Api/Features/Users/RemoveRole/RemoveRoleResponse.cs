namespace Login.Api.Features.Users.RemoveRole;

public class RemoveRoleResponse
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string Message { get; set; } = string.Empty;
}
