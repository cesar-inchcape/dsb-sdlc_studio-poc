using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Schedules.Blackout;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Schedules.Blackout;

public class BlackoutDateCommandTests
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
    public async Task Handle_CreateBlackoutDate_CreatesSuccessfully()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var handler = new CreateBlackoutDateCommandHandler(context);

        var startDate = DateTime.Today.AddDays(10);
        var endDate = DateTime.Today.AddDays(15);
        
        var command = new CreateBlackoutDateCommand
        {
            WorkshopId = workshop.Id,
            StartDate = startDate,
            EndDate = endDate,
            Reason = "Maintenance Period",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StartDate.Should().Be(startDate.ToString("yyyy-MM-dd"));
        result.EndDate.Should().Be(endDate.ToString("yyyy-MM-dd"));
        result.Reason.Should().Be("Maintenance Period");

        var blackoutDate = context.WorkshopBlackoutDates.First(b => b.WorkshopId == workshop.Id);
        blackoutDate.StartDate.Should().Be(startDate.Date);
        blackoutDate.EndDate.Should().Be(endDate.Date);
    }

    [Fact]
    public async Task Handle_CreateBlackoutDate_OverlappingRange_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();

        // Create existing blackout
        var existingBlackout = new WorkshopBlackoutDate
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(15),
            Reason = "Existing Blackout"
        };

        context.WorkshopBlackoutDates.Add(existingBlackout);
        context.SaveChanges();

        var handler = new CreateBlackoutDateCommandHandler(context);
        var command = new CreateBlackoutDateCommand
        {
            WorkshopId = workshop.Id,
            StartDate = DateTime.Today.AddDays(12),
            EndDate = DateTime.Today.AddDays(17),
            Reason = "Overlapping Blackout",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreateBlackoutDate_PastDate_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var handler = new CreateBlackoutDateCommandHandler(context);

        var command = new CreateBlackoutDateCommand
        {
            WorkshopId = workshop.Id,
            StartDate = DateTime.Today.AddDays(-5),
            EndDate = DateTime.Today.AddDays(-1),
            Reason = "Past Blackout",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GetBlackoutDates_WithDateRange_ReturnsFiltered()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();

        // Create blackout dates
        var blackout1 = new WorkshopBlackoutDate
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(8),
            Reason = "Blackout 1"
        };

        var blackout2 = new WorkshopBlackoutDate
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            StartDate = DateTime.Today.AddDays(20),
            EndDate = DateTime.Today.AddDays(25),
            Reason = "Blackout 2"
        };

        context.WorkshopBlackoutDates.AddRange(blackout1, blackout2);
        context.SaveChanges();

        var handler = new GetBlackoutDatesQueryHandler(context);
        var query = new GetBlackoutDatesQuery
        {
            WorkshopId = workshop.Id,
            FromDate = DateTime.Today.AddDays(1),
            ToDate = DateTime.Today.AddDays(15)
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.BlackoutDates.Should().HaveCount(1);
        result.BlackoutDates[0].Reason.Should().Be("Blackout 1");
    }

    [Fact]
    public async Task Handle_DeleteBlackoutDate_DeletesSuccessfully()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();

        var blackoutDate = new WorkshopBlackoutDate
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(15),
            Reason = "Blackout to Delete"
        };

        context.WorkshopBlackoutDates.Add(blackoutDate);
        context.SaveChanges();

        var handler = new DeleteBlackoutDateCommandHandler(context);
        var command = new DeleteBlackoutDateCommand
        {
            BlackoutDateId = blackoutDate.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("deleted");

        var deletedBlackout = context.WorkshopBlackoutDates.FirstOrDefault(b => b.Id == blackoutDate.Id);
        deletedBlackout.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeleteBlackoutDate_NonSuperAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();

        var blackoutDate = new WorkshopBlackoutDate
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(15),
            Reason = "Blackout"
        };

        context.WorkshopBlackoutDates.Add(blackoutDate);
        context.SaveChanges();

        var handler = new DeleteBlackoutDateCommandHandler(context);
        var command = new DeleteBlackoutDateCommand
        {
            BlackoutDateId = blackoutDate.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
