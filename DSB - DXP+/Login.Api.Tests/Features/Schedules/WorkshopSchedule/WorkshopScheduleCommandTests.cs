using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Schedules.WorkshopSchedule;
using Microsoft.EntityFrameworkCore;
using WorkshopScheduleEntity = Login.Api.Infrastructure.Data.Entities.WorkshopSchedule;

namespace Login.Api.Tests.Features.Schedules.WorkshopSchedule;

public class WorkshopScheduleCommandTests
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
    public async Task Handle_UpdateSchedule_CreatesIfNotExists()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var handler = new UpdateWorkshopScheduleCommandHandler(context);

        var command = new UpdateWorkshopScheduleCommand
        {
            WorkshopId = workshop.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            IsOpen = true,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DayOfWeek.Should().Be("Monday");
        result.StartTime.Should().Be("08:00");
        result.EndTime.Should().Be("18:00");

        var schedule = context.WorkshopSchedules.First(s => s.DayOfWeek == DayOfWeek.Monday);
        schedule.IsOpen.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UpdateSchedule_UpdatesExisting()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        
        // Create initial schedule
        var initialSchedule = new WorkshopScheduleEntity
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsOpen = true
        };
        
        context.WorkshopSchedules.Add(initialSchedule);
        context.SaveChanges();

        var handler = new UpdateWorkshopScheduleCommandHandler(context);
        var command = new UpdateWorkshopScheduleCommand
        {
            WorkshopId = workshop.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            IsOpen = true,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.StartTime.Should().Be("08:00");
        result.EndTime.Should().Be("18:00");

        var updatedSchedule = context.WorkshopSchedules.First(s => s.DayOfWeek == DayOfWeek.Monday);
        updatedSchedule.StartTime.Should().Be(new TimeSpan(8, 0, 0));
        updatedSchedule.EndTime.Should().Be(new TimeSpan(18, 0, 0));
    }

    [Fact]
    public async Task Handle_UpdateSchedule_WorkshopNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new UpdateWorkshopScheduleCommandHandler(context);

        var command = new UpdateWorkshopScheduleCommand
        {
            WorkshopId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            IsOpen = true,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UpdateSchedule_NonSuperAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var handler = new UpdateWorkshopScheduleCommandHandler(context);

        var command = new UpdateWorkshopScheduleCommand
        {
            WorkshopId = workshop.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            IsOpen = true,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GetSchedules_ReturnsAllDays()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        
        // Create schedules for all days
        for (int i = 0; i < 7; i++)
        {
            var schedule = new WorkshopScheduleEntity
            {
                Id = Guid.NewGuid(),
                WorkshopId = workshop.Id,
                DayOfWeek = (DayOfWeek)i,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                IsOpen = i < 6 // Closed on Sunday
            };
            
            context.WorkshopSchedules.Add(schedule);
        }
        
        context.SaveChanges();

        var handler = new GetWorkshopScheduleQueryHandler(context);
        var query = new GetWorkshopScheduleQuery { WorkshopId = workshop.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Schedules.Should().HaveCount(7);
        result.Schedules[6].IsOpen.Should().BeFalse(); // Sunday
    }

    [Fact]
    public async Task Handle_GetSchedules_WorkshopNotFound_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopScheduleQueryHandler(context);

        var query = new GetWorkshopScheduleQuery { WorkshopId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }
}
