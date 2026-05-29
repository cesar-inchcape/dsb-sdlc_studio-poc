using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Workshops.Update;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Workshops.Update;

public class UpdateWorkshopCommandTests
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
    public async Task Handle_WithValidData_UpdatesWorkshop()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new UpdateWorkshopCommandHandler(context);
        var workshop = context.Workshops.First();

        var command = new UpdateWorkshopCommand
        {
            WorkshopId = workshop.Id,
            Name = "Suzuki Downtown Updated",
            Capacity = 60,
            Address = new UpdateWorkshopAddress
            {
                Street = "Av. Alameda 2000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Suzuki Downtown Updated");
        result.Capacity.Should().Be(60);

        var updatedWorkshop = context.Workshops.First(w => w.Id == workshop.Id);
        updatedWorkshop.Name.Should().Be("Suzuki Downtown Updated");
        updatedWorkshop.Capacity.Should().Be(60);
        updatedWorkshop.Address!.Street.Should().Be("Av. Alameda 2000");
    }

    [Fact]
    public async Task Handle_WorkshopNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new UpdateWorkshopCommandHandler(context);

        var command = new UpdateWorkshopCommand
        {
            WorkshopId = Guid.NewGuid(),
            Name = "New Name",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InactiveWorkshop_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        workshop.IsActive = false;
        context.SaveChanges();

        var handler = new UpdateWorkshopCommandHandler(context);

        var command = new UpdateWorkshopCommand
        {
            WorkshopId = workshop.Id,
            Name = "New Name",
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
        var handler = new UpdateWorkshopCommandHandler(context);

        var command = new UpdateWorkshopCommand
        {
            WorkshopId = workshop.Id,
            Name = "New Name",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PartialUpdate_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var originalCapacity = workshop.Capacity;
        var handler = new UpdateWorkshopCommandHandler(context);

        var command = new UpdateWorkshopCommand
        {
            WorkshopId = workshop.Id,
            Name = "Updated Name",
            Capacity = null, // Don't update capacity
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Capacity.Should().Be(originalCapacity); // Should remain unchanged

        var updatedWorkshop = context.Workshops.First(w => w.Id == workshop.Id);
        updatedWorkshop.Capacity.Should().Be(originalCapacity);
    }
}
