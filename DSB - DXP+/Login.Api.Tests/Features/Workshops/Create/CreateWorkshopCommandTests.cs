using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Workshops.Create;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Workshops.Create;

public class CreateWorkshopCommandTests
{
    private LoginDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new LoginDbContext(options);
        return context;
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesWorkshop()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateWorkshopCommandHandler(context);

        var command = new CreateWorkshopCommand
        {
            Name = "Suzuki Workshop Santiago",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Address = new CreateWorkshopAddress
            {
                Street = "Av. Alameda 1000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 50,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Suzuki Workshop Santiago");
        result.Brand.Should().Be("Suzuki");
        result.Location.Should().Be("Downtown Santiago");
        result.Capacity.Should().Be(50);

        // Verify workshop and schedules were created
        var workshop = context.Workshops.FirstOrDefault(w => w.Id == result.Id);
        workshop.Should().NotBeNull();
        workshop!.IsActive.Should().BeTrue();
        
        var schedules = context.WorkshopSchedules.Where(s => s.WorkshopId == workshop.Id).ToList();
        schedules.Should().HaveCount(7); // One for each day of week
    }

    [Fact]
    public async Task Handle_DuplicateWorkshopNameAndBrand_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateWorkshopCommandHandler(context);

        var existingWorkshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Name = "Suzuki Workshop Santiago",
            Brand = WorkshopBrand.Suzuki,
            Location = "Old Location",
            Capacity = 50,
            Address = new Address
            {
                Street = "Old Street",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Workshops.Add(existingWorkshop);
        context.SaveChanges();

        var command = new CreateWorkshopCommand
        {
            Name = "Suzuki Workshop Santiago",
            Brand = WorkshopBrand.Suzuki,
            Location = "New Location",
            Address = new CreateWorkshopAddress
            {
                Street = "New Street",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 50,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateLocationPerBrand_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateWorkshopCommandHandler(context);

        var existingWorkshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Name = "Existing Workshop",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Capacity = 50,
            Address = new Address
            {
                Street = "Av. Alameda 1000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Workshops.Add(existingWorkshop);
        context.SaveChanges();

        var command = new CreateWorkshopCommand
        {
            Name = "New Workshop",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Address = new CreateWorkshopAddress
            {
                Street = "Av. Alameda 2000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 50,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SameLocationDifferentBrand_Succeeds()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateWorkshopCommandHandler(context);

        var existingWorkshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Name = "Suzuki Workshop",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Capacity = 50,
            Address = new Address
            {
                Street = "Av. Alameda 1000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Workshops.Add(existingWorkshop);
        context.SaveChanges();

        var command = new CreateWorkshopCommand
        {
            Name = "Changan Workshop",
            Brand = WorkshopBrand.Changan, // Different brand
            Location = "Downtown Santiago",
            Address = new CreateWorkshopAddress
            {
                Street = "Av. Alameda 2000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 50,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Brand.Should().Be("Changan");
    }

    [Fact]
    public async Task Handle_NonSuperAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateWorkshopCommandHandler(context);

        var command = new CreateWorkshopCommand
        {
            Name = "Suzuki Workshop",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Address = new CreateWorkshopAddress
            {
                Street = "Av. Alameda 1000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 50,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ZeroCapacity_FailsValidation()
    {
        // This test verifies the validator catches zero capacity
        var validator = new CreateWorkshopValidator();
        var command = new CreateWorkshopCommand
        {
            Name = "Suzuki Workshop",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Address = new CreateWorkshopAddress
            {
                Street = "Av. Alameda 1000",
                City = "Santiago",
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 0,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Capacity");
    }

    [Fact]
    public async Task Handle_InvalidAddress_FailsValidation()
    {
        // This test verifies the validator catches incomplete address
        var validator = new CreateWorkshopValidator();
        var command = new CreateWorkshopCommand
        {
            Name = "Suzuki Workshop",
            Brand = WorkshopBrand.Suzuki,
            Location = "Downtown Santiago",
            Address = new CreateWorkshopAddress
            {
                Street = "Av. Alameda 1000",
                City = "",  // Empty city
                Region = "Metropolitan",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 50,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Address");
    }
}
