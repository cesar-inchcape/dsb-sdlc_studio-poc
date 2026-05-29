using Xunit;
using FluentAssertions;
using Moq;
using Login.Api.Features.Auth.Login;
using Login.Api.Infrastructure.Security;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Login.Api.Tests.Features.Auth;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly LoginDbContext _dbContext;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LoginDbContext(options);
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _handler = new LoginCommandHandler(_dbContext, _tokenGeneratorMock.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var superAdminRole = new Role
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "SuperAdmin",
            Description = "Platform-wide control"
        };

        var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test123!", workFactor: 12);

        var testUser = new User
        {
            Id = testUserId,
            Email = "admin@test.com",
            PasswordHash = passwordHash,
            FirstName = "Admin",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var inactiveUser = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "inactive@test.com",
            PasswordHash = passwordHash,
            FirstName = "Inactive",
            LastName = "User",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Roles.Add(superAdminRole);
        _dbContext.Users.AddRange(testUser, inactiveUser);
        
        _dbContext.UserRoles.Add(new UserRole
        {
            UserId = testUserId,
            RoleId = superAdminRole.Id
        });

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponseWithTokens()
    {
        // Arrange
        var command = new LoginCommand("admin@test.com", "Test123!");
        var expectedAccessToken = "mock-access-token";
        var expectedRefreshToken = "mock-refresh-token";

        _tokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(expectedAccessToken);

        _tokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(expectedRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(expectedAccessToken);
        result.RefreshToken.Should().Be(expectedRefreshToken);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsUserWithRoles()
    {
        // Arrange
        var command = new LoginCommand("admin@test.com", "Test123!");

        _tokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("token");

        _tokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("admin@test.com");
        result.User.FirstName.Should().Be("Admin");
        result.User.LastName.Should().Be("User");
        result.User.Roles.Should().Contain("SuperAdmin");
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@test.com", "Test123!");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand("admin@test.com", "WrongPassword!");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand("inactive@test.com", "Test123!");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_ValidCredentials_StoresRefreshTokenInDatabase()
    {
        // Arrange
        var command = new LoginCommand("admin@test.com", "Test123!");
        var expectedRefreshToken = "mock-refresh-token";

        _tokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("token");

        _tokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(expectedRefreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == expectedRefreshToken);

        storedToken.Should().NotBeNull();
        storedToken!.UserId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        storedToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
        storedToken.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NullEmail_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new LoginCommand(null!, "password");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_EmptyPassword_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new LoginCommand("test@test.com", "");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_PasswordHashingValidation_UsesBCrypt()
    {
        // Arrange
        var command = new LoginCommand("admin@test.com", "Test123!");

        _tokenGeneratorMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("token");

        _tokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - If we get here without exception, BCrypt validation passed
        result.Should().NotBeNull();
        
        // Verify the user's password hash is BCrypt format (starts with $2a$ or $2b$)
        var user = await _dbContext.Users.FindAsync(result.User.Id);
        user!.PasswordHash.Should().Match(hash => 
            hash.StartsWith("$2a$") || hash.StartsWith("$2b$"));
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
