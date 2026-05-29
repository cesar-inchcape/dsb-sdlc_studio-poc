using FluentAssertions;
using Xunit;
using Login.Api.Features.Users.Update;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Login.Api.Tests.Features.Users;

public class UpdateUserCommandTests : IDisposable
{
    private readonly LoginDbContext _dbContext;
    private readonly UpdateUserCommandHandler _handler;
    private readonly Guid _superAdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private readonly Guid _distributorAdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private readonly Guid _workshopUserRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public UpdateUserCommandTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LoginDbContext(options);
        _handler = new UpdateUserCommandHandler(_dbContext);

        // Seed initial data
        SeedInitialData();
    }

    private void SeedInitialData()
    {
        var roles = new List<Role>
        {
            new() { Id = _superAdminRoleId, Name = "SuperAdmin", Description = "Platform admin" },
            new() { Id = _distributorAdminRoleId, Name = "DistributorAdmin", Description = "Distributor admin" },
            new() { Id = _workshopUserRoleId, Name = "WorkshopUser", Description = "Workshop user" }
        };

        _dbContext.Roles.AddRange(roles);

        // Create test users
        var adminUser = new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email = "admin@dsb.cl",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole>()
        };

        var existingUser = new User
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Email = "existing@dsb.cl",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Existing123!", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole>
            {
                new() { UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), RoleId = _workshopUserRoleId }
            }
        };

        _dbContext.Users.AddRange(adminUser, existingUser);
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    #region Positive Cases

    [Fact]
    public async Task Handle_ValidRequest_UpdatesUserSuccessfully()
    {
        // Arrange
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "UpdatedFirst",
            LastName = "UpdatedLast",
            RequesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RequesterRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("UpdatedFirst");
        result.LastName.Should().Be("UpdatedLast");

        // Verify in database
        var updatedUser = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be("UpdatedFirst");
    }

    [Fact]
    public async Task Handle_UpdateEmail_ValidatesUniqueness()
    {
        // Arrange - Try to update with an email that doesn't exist
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var command = new UpdateUserCommand
        {
            UserId = userId,
            Email = "newemail@dsb.cl",
            FirstName = "Existing",
            LastName = "User",
            RequesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RequesterRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Email.Should().Be("newemail@dsb.cl");
        var updatedUser = _dbContext.Users.FirstOrDefault(u => u.Email == "newemail@dsb.cl");
        updatedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_UpdateEmail_ThrowsIfDuplicateExists()
    {
        // Arrange - Create another user first
        var newUser = new User
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Email = "another@dsb.cl",
            FirstName = "Another",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Another123!", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();

        // Try to update first user with existing email
        var command = new UpdateUserCommand
        {
            UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Email = "another@dsb.cl",
            FirstName = "Existing",
            LastName = "User",
            RequesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RequesterRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Email already exists");
    }

    #endregion

    #region Authorization Cases

    [Fact]
    public async Task Handle_SuperAdminCanUpdateAnyUser()
    {
        // Arrange
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "UpdatedByAdmin",
            LastName = "User",
            RequesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RequesterRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FirstName.Should().Be("UpdatedByAdmin");
    }

    [Fact]
    public async Task Handle_UserCanOnlyUpdateOwnProfile()
    {
        // Arrange - User tries to update someone else
        var otherId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var requesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        
        var command = new UpdateUserCommand
        {
            UserId = otherId,
            FirstName = "Hacked",
            LastName = "Name",
            RequesterId = requesterId,
            RequesterRoles = new List<string> { "WorkshopUser" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Cannot update other user's profile");
    }

    [Fact]
    public async Task Handle_UserCanUpdateOwnProfile()
    {
        // Arrange - User updates own profile
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "MyNewName",
            LastName = "MyNewLast",
            RequesterId = userId,
            RequesterRoles = new List<string> { "WorkshopUser" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FirstName.Should().Be("MyNewName");
    }

    #endregion

    #region Validation Cases

    [Fact]
    public async Task Handle_NonExistentUser_ThrowsException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = nonExistentId,
            FirstName = "Updated",
            LastName = "User",
            RequesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RequesterRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("User not found");
    }

    [Fact]
    public async Task Handle_InactiveUser_CannotBeUpdated()
    {
        // Arrange - Create inactive user
        var inactiveUser = new User
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            Email = "inactive@dsb.cl",
            FirstName = "Inactive",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Inactive123!", workFactor: 12),
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(inactiveUser);
        _dbContext.SaveChanges();

        var command = new UpdateUserCommand
        {
            UserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            FirstName = "Updated",
            LastName = "User",
            RequesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RequesterRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Cannot update inactive user");
    }

    [Fact]
    public async Task Handle_EmptyFirstName_ThrowsException()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            FirstName = "",
            LastName = "User",
            RequesterId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RequesterRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("FirstName");
    }

    #endregion
}
