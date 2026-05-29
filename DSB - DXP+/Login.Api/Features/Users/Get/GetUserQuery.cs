using MediatR;

namespace Login.Api.Features.Users.Get;

public class GetUserQuery : IRequest<UserDto?>
{
    public required Guid UserId { get; set; }
}
