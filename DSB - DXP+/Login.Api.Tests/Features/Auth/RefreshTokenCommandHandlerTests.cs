using FluentAssertions;
using Login.Api.Features.Auth.RefreshToken;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Login.Api.Tests.Features.Auth;

public class RefreshTokenCommandHandlerTests : IDisposable
{
    private readonly LoginDbContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly RefreshTokenCommandHandler _handler;
    private readonly User _testUser;
    private readonly string _validRefreshToken;

    public RefreshTokenCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new LoginDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Setup token generator with configuration
        var configData = new Dictionary<string, string>
        {
            {"Jwt:Secret", "this-is-a-very-long-secret-key-for-testing-purposes-at-least-32-characters"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiryMinutes", "60"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        _tokenGenerator = new JwtTokenGenerator(configuration);

        // Setup handler
        _handler = new RefreshTokenCommandHandler(_dbContext, _tokenGenerator);

        // Create test user with roles
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "SuperAdmin",
            Description = "Super Administrator"
        };

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword123!"),
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userRole = new UserRole
        {
            UserId = _testUser.Id,
            RoleId = role.Id,
            User = _testUser,
            Role = role
        };

        _testUser.UserRoles = new List<UserRole> { userRole };

        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(_testUser);
        _dbContext.UserRoles.Add(userRole);

        // Create valid refresh token
        _validRefreshToken = _tokenGenerator.GenerateRefreshToken();
        var refreshTokenEntity = new Login.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _testUser.Id,
            Token = _validRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewAccessToken()
    {
        // Arrange
        var command = new RefreshTokenCommand(_validRefreshToken);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_NewAccessTokenContainsCorrectClaims()
    {
        // Arrange
        var command = new RefreshTokenCommand(_validRefreshToken);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var principal = _tokenGenerator.ValidateToken(response.AccessToken);
        principal.Should().NotBeNull();
        
        var userId = principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        userId.Should().Be(_testUser.Id.ToString());

        var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        email.Should().Be(_testUser.Email);

        var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        role.Should().Be("SuperAdmin");
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_OldRefreshTokenIsRevoked()
    {
        // Arrange
        var command = new RefreshTokenCommand(_validRefreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var revokedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == _validRefreshToken);
        
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();
        revokedToken.RevokedAt.Should().NotBeNull();
        revokedToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_NewRefreshTokenIsStoredInDatabase()
    {
        // Arrange
        var command = new RefreshTokenCommand(_validRefreshToken);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var newToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == response.RefreshToken && !rt.IsRevoked);
        
        newToken.Should().NotBeNull();
        newToken!.UserId.Should().Be(_testUser.Id);
        newToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_ExpiredRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var expiredToken = _tokenGenerator.GenerateRefreshToken();
        var expiredRefreshTokenEntity = new Login.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _testUser.Id,
            Token = expiredToken,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(expiredRefreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand(expiredToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new RefreshTokenCommand("invalid-token-12345");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_RevokedRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var revokedToken = _tokenGenerator.GenerateRefreshToken();
        var revokedRefreshTokenEntity = new Login.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _testUser.Id,
            Token = revokedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true, // Already revoked
            RevokedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _dbContext.RefreshTokens.Add(revokedRefreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand(revokedToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_NullRefreshToken_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new RefreshTokenCommand(null!);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_EmptyRefreshToken_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new RefreshTokenCommand(string.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
