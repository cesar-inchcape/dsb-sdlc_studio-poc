using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Advisors.Create;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Advisors.Create;

public class CreateAdvisorCommandTests
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
            Name = "Test Workshop",
            Brand = WorkshopBrand.Suzuki,
            Location = "Test Location",
            Capacity = 50,
            Address = new Address { Street = "Test St", City = "Test City", Region = "Test", PostalCode = "12345", Country = "Chile" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Workshops.Add(workshop);
        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesAdvisor()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateAdvisorCommandHandler(context);
        var workshop = context.Workshops.First();

        var command = new CreateAdvisorCommand
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@dsb.cl",
            PhoneNumber = "+56912345678",
            WorkshopId = workshop.Id,
            AssignedBrand = AdvisorBrand.Suzuki,
            AvailableHoursPerDay = 8,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Juan");
        result.Email.Should().Be("juan@dsb.cl");
        result.AssignedBrand.Should().Be("Suzuki");

        var createdAdvisor = context.Advisors.First(a => a.Email == "juan@dsb.cl");
        createdAdvisor.Should().NotBeNull();
        createdAdvisor.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateAdvisorCommandHandler(context);
        var workshop = context.Workshops.First();

        var command1 = new CreateAdvisorCommand
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@dsb.cl",
            PhoneNumber = "+56912345678",
            WorkshopId = workshop.Id,
            AssignedBrand = AdvisorBrand.Suzuki,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        await handler.Handle(command1, CancellationToken.None);

        var command2 = new CreateAdvisorCommand
        {
            FirstName = "Carlos",
            LastName = "González",
            Email = "juan@dsb.cl",
            PhoneNumber = "+56987654321",
            WorkshopId = workshop.Id,
            AssignedBrand = AdvisorBrand.Changan,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command2, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonSuperAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateAdvisorCommandHandler(context);
        var workshop = context.Workshops.First();

        var command = new CreateAdvisorCommand
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@dsb.cl",
            PhoneNumber = "+56912345678",
            WorkshopId = workshop.Id,
            AssignedBrand = AdvisorBrand.Suzuki,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WorkshopNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new CreateAdvisorCommandHandler(context);

        var command = new CreateAdvisorCommand
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@dsb.cl",
            PhoneNumber = "+56912345678",
            WorkshopId = Guid.NewGuid(),
            AssignedBrand = AdvisorBrand.Suzuki,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
