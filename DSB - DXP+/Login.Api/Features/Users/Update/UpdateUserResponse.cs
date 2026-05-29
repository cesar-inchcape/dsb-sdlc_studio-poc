namespace Login.Api.Features.Users.Update;

public class UpdateUserResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = [];
    public DateTime UpdatedAt { get; set; }
}
