using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Infrastructure.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly LoginDbContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator;

    public LoginCommandHandler(LoginDbContext dbContext, ITokenGenerator tokenGenerator)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentNullException(nameof(request.Email), "Email cannot be null or empty");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentNullException(nameof(request.Password), "Password cannot be null or empty");

        // Find user by email with roles
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Generic error for security (prevent user enumeration)
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Verify password
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Check if user is active
        if (!user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Extract roles
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();

        // Generate tokens
        var accessToken = _tokenGenerator.GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        // Store refresh token in database
        var refreshTokenEntity = new Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Build response
        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roles
        );

        return new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60),
            userDto
        );
    }
}
