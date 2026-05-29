using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Advisors.Delete;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Advisors.Delete;

public class DeleteAdvisorCommandTests
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
    public async Task Handle_WithValidId_DeactivatesAdvisor()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new DeleteAdvisorCommandHandler(context);
        var advisor = context.Advisors.First();

        var command = new DeleteAdvisorCommand
        {
            AdvisorId = advisor.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(advisor.Id);
        result.Message.Should().Contain("deactivated");

        var deletedAdvisor = context.Advisors.First(a => a.Id == advisor.Id);
        deletedAdvisor.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AdvisorNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new DeleteAdvisorCommandHandler(context);

        var command = new DeleteAdvisorCommand
        {
            AdvisorId = Guid.NewGuid(),
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
        var advisor = context.Advisors.First();
        advisor.IsActive = false;
        context.SaveChanges();

        var handler = new DeleteAdvisorCommandHandler(context);

        var command = new DeleteAdvisorCommand
        {
            AdvisorId = advisor.Id,
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
        var handler = new DeleteAdvisorCommandHandler(context);

        var command = new DeleteAdvisorCommand
        {
            AdvisorId = advisor.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
