using FluentAssertions;
using Xunit;
using Login.Api.Features.Users.Create;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Users;

public class CreateUserCommandTests : IDisposable
{
    private readonly LoginDbContext _dbContext;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LoginDbContext(options);
        _handler = new CreateUserCommandHandler(_dbContext);

        // Seed initial data
        SeedInitialData();
    }

    private void SeedInitialData()
    {
        var workshopUserRole = new Role
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "WorkshopUser",
            Description = "Workshop user role"
        };

        _dbContext.Roles.Add(workshopUserRole);
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    #region Positive Cases

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@dsb.cl",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Email.Should().Be("newuser@dsb.cl");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.IsActive.Should().BeTrue();

        // Verify in database
        var savedUser = _dbContext.Users.FirstOrDefault(u => u.Email == "newuser@dsb.cl");
        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task Handle_WithRoles_AssignsRolesDuringCreation()
    {
        // Arrange
        var roleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var command = new CreateUserCommand
        {
            Email = "userwithrole@dsb.cl",
            FirstName = "Jane",
            LastName = "Smith",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid> { roleId }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().HaveCount(1);
        result.Roles.First().Should().Be("WorkshopUser");

        // Verify in database
        var savedUser = _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Email == "userwithrole@dsb.cl");
        savedUser.Should().NotBeNull();
        savedUser!.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_PasswordIsHashedWithBCrypt()
    {
        // Arrange
        var plainPassword = "SecurePassword123!";
        var command = new CreateUserCommand
        {
            Email = "bcryptuser@dsb.cl",
            FirstName = "Hash",
            LastName = "Test",
            Password = plainPassword,
            RoleIds = new List<Guid>()
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedUser = _dbContext.Users.FirstOrDefault(u => u.Email == "bcryptuser@dsb.cl");
        savedUser.Should().NotBeNull();
        savedUser!.PasswordHash.Should().NotBe(plainPassword);
        savedUser.PasswordHash.Should().StartWith("$2");
        BCrypt.Net.BCrypt.Verify(plainPassword, savedUser.PasswordHash).Should().BeTrue();
    }

    #endregion

    #region Validation Cases

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsValidationException()
    {
        // Arrange - Create first user
        var firstCommand = new CreateUserCommand
        {
            Email = "duplicate@dsb.cl",
            FirstName = "First",
            LastName = "User",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid>()
        };
        await _handler.Handle(firstCommand, CancellationToken.None);

        // Arrange - Try to create second user with same email
        var duplicateCommand = new CreateUserCommand
        {
            Email = "duplicate@dsb.cl",
            FirstName = "Second",
            LastName = "User",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(duplicateCommand, CancellationToken.None));
        exception.Message.Should().Contain("Email already exists");
    }

    [Fact]
    public async Task Handle_EmptyEmail_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Email is required");
    }

    [Fact]
    public async Task Handle_InvalidEmailFormat_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "invalid-email",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Invalid email format");
    }

    [Fact]
    public async Task Handle_WeakPassword_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@dsb.cl",
            FirstName = "John",
            LastName = "Doe",
            Password = "weak",  // Too short and not complex
            RoleIds = new List<Guid>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Password must be at least 8 characters");
    }

    [Fact]
    public async Task Handle_EmptyFirstName_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@dsb.cl",
            FirstName = "",
            LastName = "Doe",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("FirstName is required");
    }

    [Fact]
    public async Task Handle_InvalidRoleId_ThrowsValidationException()
    {
        // Arrange
        var invalidRoleId = Guid.NewGuid();
        var command = new CreateUserCommand
        {
            Email = "newuser@dsb.cl",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!",
            RoleIds = new List<Guid> { invalidRoleId }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Role not found");
    }

    #endregion
}
