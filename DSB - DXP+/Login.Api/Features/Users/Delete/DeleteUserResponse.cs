namespace Login.Api.Features.Users.Delete;

public class DeleteUserResponse
{
    public Guid UserId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = "User deactivated successfully";
}
