using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly LoginDbContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator;

    public RefreshTokenCommandHandler(LoginDbContext dbContext, ITokenGenerator tokenGenerator)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentNullException(nameof(request.RefreshToken), "Refresh token cannot be null or empty");

        // Find refresh token in database
        var refreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        // Validate token exists
        if (refreshToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Check if token is revoked
        if (refreshToken.IsRevoked)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Check if token is expired
        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Get user and roles
        var user = refreshToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();

        // Generate new access token
        var newAccessToken = _tokenGenerator.GenerateAccessToken(user.Id, user.Email, roles);

        // Generate new refresh token
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

        // Revoke old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Store new refresh token in database
        var newRefreshTokenEntity = new Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Return response
        return new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(60)
        );
    }
}
