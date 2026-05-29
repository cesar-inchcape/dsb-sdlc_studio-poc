using MediatR;

namespace Login.Api.Features.Users.Create;

public class CreateUserCommand : IRequest<CreateUserResponse>
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
    public List<Guid>? RoleIds { get; set; }
}
