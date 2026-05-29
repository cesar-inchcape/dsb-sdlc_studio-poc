using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Users.RemoveRole;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Users.RemoveRole;

public class RemoveRoleCommandTests
{
    private LoginDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new LoginDbContext(options);
        SeedData(context);
        return context;
    }

    private void SeedData(LoginDbContext context)
    {
        var superAdminRole = new Role { Id = Guid.NewGuid(), Name = "SuperAdmin" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "DistributorAdmin" };
        var workshopRole = new Role { Id = Guid.NewGuid(), Name = "WorkshopUser" };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@dsb.cl",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(superAdminRole, adminRole, workshopRole);
        context.Users.Add(user);
        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidData_RemovesRoleFromUser()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new RemoveRoleCommandHandler(context);
        
        var user = context.Users.First();
        var role = context.Roles.First();

        // Pre-assign role
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        context.SaveChanges();

        var command = new RemoveRoleCommand
        {
            UserId = user.Id,
            RoleId = role.Id,
            RequestingUserId = user.Id,
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.RoleId.Should().Be(role.Id);
        result.Message.Should().Contain("removed from user");

        var userRole = context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        userRole.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new RemoveRoleCommandHandler(context);
        var role = context.Roles.First();
        var invalidUserId = Guid.NewGuid();

        var command = new RemoveRoleCommand
        {
            UserId = invalidUserId,
            RoleId = role.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RoleNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new RemoveRoleCommandHandler(context);
        var user = context.Users.First();
        var invalidRoleId = Guid.NewGuid();

        var command = new RemoveRoleCommand
        {
            UserId = user.Id,
            RoleId = invalidRoleId,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserDoesNotHaveRole_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new RemoveRoleCommandHandler(context);
        var user = context.Users.First();
        var role = context.Roles.First();

        var command = new RemoveRoleCommand
        {
            UserId = user.Id,
            RoleId = role.Id,
            RequestingUserId = user.Id,
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonSuperAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new RemoveRoleCommandHandler(context);
        var user = context.Users.First();
        var role = context.Roles.First();

        var command = new RemoveRoleCommand
        {
            UserId = user.Id,
            RoleId = role.Id,
            RequestingUserId = user.Id,
            RequestingUserRoles = new List<string> { "DistributorAdmin" } // Not SuperAdmin
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
