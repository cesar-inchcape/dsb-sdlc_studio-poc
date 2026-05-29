using FluentAssertions;
using Xunit;
using Login.Api.Features.Users.Get;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Users;

public class GetUserQueryTests : IDisposable
{
    private readonly LoginDbContext _dbContext;
    private readonly GetUserQueryHandler _handler;
    private readonly GetUsersQueryHandler _listHandler;
    private readonly Guid _superAdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private readonly Guid _workshopUserRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public GetUserQueryTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LoginDbContext(options);
        _handler = new GetUserQueryHandler(_dbContext);
        _listHandler = new GetUsersQueryHandler(_dbContext);

        // Seed initial data
        SeedInitialData();
    }

    private void SeedInitialData()
    {
        var roles = new List<Role>
        {
            new() { Id = _superAdminRoleId, Name = "SuperAdmin", Description = "Platform admin" },
            new() { Id = _workshopUserRoleId, Name = "WorkshopUser", Description = "Workshop user" }
        };

        _dbContext.Roles.AddRange(roles);

        // Create test users
        var admin = new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email = "admin@dsb.cl",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole>
            {
                new() { UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), RoleId = _superAdminRoleId }
            }
        };

        var user1 = new User
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Email = "user1@dsb.cl",
            FirstName = "User",
            LastName = "One",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            UserRoles = new List<UserRole>
            {
                new() { UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), RoleId = _workshopUserRoleId }
            }
        };

        var user2 = new User
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Email = "user2@dsb.cl",
            FirstName = "User",
            LastName = "Two",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = null,
            UserRoles = new List<UserRole>
            {
                new() { UserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), RoleId = _workshopUserRoleId }
            }
        };

        var inactiveUser = new User
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            Email = "inactive@dsb.cl",
            FirstName = "Inactive",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Inactive123!", workFactor: 12),
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-15),
            UserRoles = []
        };

        _dbContext.Users.AddRange(admin, user1, user2, inactiveUser);
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    #region GetUserQuery Tests

    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserWithRoles()
    {
        // Arrange
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var query = new GetUserQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("user1@dsb.cl");
        result.FirstName.Should().Be("User");
        result.Roles.Should().ContainSingle(r => r == "WorkshopUser");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetUserQuery { UserId = nonExistentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsUserData()
    {
        // Arrange
        var inactiveId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var query = new GetUserQuery { UserId = inactiveId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
        result.Email.Should().Be("inactive@dsb.cl");
    }

    #endregion

    #region GetUsersQuery Tests

    [Fact]
    public async Task ListHandle_NoFilters_ReturnsAllActiveUsers()
    {
        // Arrange
        var query = new GetUsersQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _listHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().HaveCount(3);  // 3 active users (admin + user1 + user2, not inactive)
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ListHandle_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var query = new GetUsersQuery { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _listHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);  // ceil(3 / 2) = 2
    }

    [Fact]
    public async Task ListHandle_SecondPage_ReturnsRemainingUsers()
    {
        // Arrange
        var query = new GetUsersQuery { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _listHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().HaveCount(1);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task ListHandle_EachUserHasRoles()
    {
        // Arrange
        var query = new GetUsersQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _listHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Users.Should().AllSatisfy(u =>
        {
            u.Id.Should().NotBeEmpty();
            u.Email.Should().NotBeNullOrEmpty();
            u.FirstName.Should().NotBeNullOrEmpty();
            u.Roles.Should().BeOfType<List<string>>();
        });
    }

    [Fact]
    public async Task ListHandle_FilterByEmail_ReturnsMatchingUser()
    {
        // Arrange
        var query = new GetUsersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            EmailFilter = "user1@dsb.cl"
        };

        // Act
        var result = await _listHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(1);
        result.Users.First().Email.Should().Be("user1@dsb.cl");
    }

    [Fact]
    public async Task ListHandle_FilterByFirstName_ReturnsCaseInsensitiveMatches()
    {
        // Arrange
        var query = new GetUsersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            FirstNameFilter = "user"
        };

        // Act
        var result = await _listHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(2);  // user1 and user2
        result.Users.Should().AllSatisfy(u => u.FirstName.ToLower().Contains("user"));
    }

    #endregion
}
