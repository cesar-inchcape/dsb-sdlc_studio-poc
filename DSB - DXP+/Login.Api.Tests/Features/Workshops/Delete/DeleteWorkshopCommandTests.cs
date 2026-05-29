using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Workshops.Delete;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Workshops.Delete;

public class DeleteWorkshopCommandTests
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
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Name = "Suzuki Downtown",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Capacity = 50,
            Address = new Address { Street = "Av. Alameda 1000", City = "Santiago", Region = "Metropolitan", PostalCode = "8320000", Country = "Chile" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Workshops.Add(workshop);
        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidId_DeactivatesWorkshop()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new DeleteWorkshopCommandHandler(context);
        var workshop = context.Workshops.First();

        var command = new DeleteWorkshopCommand
        {
            WorkshopId = workshop.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(workshop.Id);
        result.Message.Should().Contain("deactivated");

        var deletedWorkshop = context.Workshops.First(w => w.Id == workshop.Id);
        deletedWorkshop.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WorkshopNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new DeleteWorkshopCommandHandler(context);

        var command = new DeleteWorkshopCommand
        {
            WorkshopId = Guid.NewGuid(),
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyInactive_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        workshop.IsActive = false;
        context.SaveChanges();

        var handler = new DeleteWorkshopCommandHandler(context);

        var command = new DeleteWorkshopCommand
        {
            WorkshopId = workshop.Id,
            RequestingUserId = Guid.NewGuid(),
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
        var workshop = context.Workshops.First();
        var handler = new DeleteWorkshopCommandHandler(context);

        var command = new DeleteWorkshopCommand
        {
            WorkshopId = workshop.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
