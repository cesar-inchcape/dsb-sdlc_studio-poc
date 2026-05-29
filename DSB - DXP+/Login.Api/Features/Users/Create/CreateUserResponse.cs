namespace Login.Api.Features.Users.Create;

public class CreateUserResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
