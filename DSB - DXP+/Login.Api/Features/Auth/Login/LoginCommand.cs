using MediatR;

namespace Login.Api.Features.Auth.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;
