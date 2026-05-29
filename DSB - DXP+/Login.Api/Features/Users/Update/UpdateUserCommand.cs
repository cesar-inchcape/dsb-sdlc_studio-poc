using MediatR;

namespace Login.Api.Features.Users.Update;

public class UpdateUserCommand : IRequest<UpdateUserResponse>
{
    public required Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
    
    // Authorization context
    public required Guid RequesterId { get; set; }
    public required List<string> RequesterRoles { get; set; }
}
