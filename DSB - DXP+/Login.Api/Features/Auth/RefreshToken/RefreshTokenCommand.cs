using MediatR;

namespace Login.Api.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;
