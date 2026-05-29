using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Advisors.Update;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Advisors.Update;

public class UpdateAdvisorCommandTests
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

        var advisor = new Advisor
        {
            Id = Guid.NewGuid(),
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@dsb.cl",
            PhoneNumber = "+56912345678",
            WorkshopId = workshop.Id,
            AssignedBrand = AdvisorBrand.Suzuki,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Advisors.Add(advisor);
        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesAdvisor()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new UpdateAdvisorCommandHandler(context);
        var advisor = context.Advisors.First();

        var command = new UpdateAdvisorCommand
        {
            AdvisorId = advisor.Id,
            FirstName = "Carlos",
            PhoneNumber = "+56987654321",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.FirstName.Should().Be("Carlos");
        result.PhoneNumber.Should().Be("+56987654321");
        result.Email.Should().Be("juan@dsb.cl"); // Unchanged

        var updatedAdvisor = context.Advisors.First(a => a.Id == advisor.Id);
        updatedAdvisor.FirstName.Should().Be("Carlos");
    }

    [Fact]
    public async Task Handle_AdvisorNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new UpdateAdvisorCommandHandler(context);

        var command = new UpdateAdvisorCommand
        {
            AdvisorId = Guid.NewGuid(),
            FirstName = "Carlos",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InactiveAdvisor_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var advisor = context.Advisors.First();
        advisor.IsActive = false;
        context.SaveChanges();

        var handler = new UpdateAdvisorCommandHandler(context);

        var command = new UpdateAdvisorCommand
        {
            AdvisorId = advisor.Id,
            FirstName = "Carlos",
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
        var advisor = context.Advisors.First();
        var handler = new UpdateAdvisorCommandHandler(context);

        var command = new UpdateAdvisorCommand
        {
            AdvisorId = advisor.Id,
            FirstName = "Carlos",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        
        var advisor2 = new Advisor
        {
            Id = Guid.NewGuid(),
            FirstName = "Carlos",
            LastName = "González",
            Email = "carlos@dsb.cl",
            PhoneNumber = "+56987654321",
            WorkshopId = workshop.Id,
            AssignedBrand = AdvisorBrand.Changan,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Advisors.Add(advisor2);
        context.SaveChanges();

        var advisor1 = context.Advisors.First(a => a.Email == "juan@dsb.cl");
        var handler = new UpdateAdvisorCommandHandler(context);

        var command = new UpdateAdvisorCommand
        {
            AdvisorId = advisor1.Id,
            Email = "carlos@dsb.cl",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
