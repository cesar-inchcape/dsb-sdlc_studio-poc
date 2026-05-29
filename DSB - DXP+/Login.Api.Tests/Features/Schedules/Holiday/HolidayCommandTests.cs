using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Schedules.Holiday;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Schedules.Holiday;

public class HolidayCommandTests
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
    public async Task Handle_CreateHoliday_CreatesSuccessfully()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var handler = new CreateHolidayCommandHandler(context);

        var futureDate = DateTime.Today.AddDays(10);
        var command = new CreateHolidayCommand
        {
            WorkshopId = workshop.Id,
            Date = futureDate,
            Reason = "National Holiday",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(futureDate.ToString("yyyy-MM-dd"));
        result.Reason.Should().Be("National Holiday");

        var holiday = context.WorkshopHolidays.First(h => h.WorkshopId == workshop.Id);
        holiday.Date.Date.Should().Be(futureDate.Date);
    }

    [Fact]
    public async Task Handle_CreateHoliday_DuplicateDate_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var futureDate = DateTime.Today.AddDays(10);

        // Create first holiday
        var existingHoliday = new WorkshopHoliday
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            Date = futureDate.Date,
            Reason = "Existing Holiday"
        };

        context.WorkshopHolidays.Add(existingHoliday);
        context.SaveChanges();

        var handler = new CreateHolidayCommandHandler(context);
        var command = new CreateHolidayCommand
        {
            WorkshopId = workshop.Id,
            Date = futureDate,
            Reason = "Another Holiday",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreateHoliday_PastDate_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();
        var handler = new CreateHolidayCommandHandler(context);

        var pastDate = DateTime.Today.AddDays(-5);
        var command = new CreateHolidayCommand
        {
            WorkshopId = workshop.Id,
            Date = pastDate,
            Reason = "Past Holiday",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GetHolidays_WithDateRange_ReturnsFiltered()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();

        // Create holidays
        var holiday1 = new WorkshopHoliday
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            Date = DateTime.Today.AddDays(5),
            Reason = "Holiday 1"
        };

        var holiday2 = new WorkshopHoliday
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            Date = DateTime.Today.AddDays(20),
            Reason = "Holiday 2"
        };

        context.WorkshopHolidays.AddRange(holiday1, holiday2);
        context.SaveChanges();

        var handler = new GetHolidaysQueryHandler(context);
        var query = new GetHolidaysQuery
        {
            WorkshopId = workshop.Id,
            FromDate = DateTime.Today.AddDays(1),
            ToDate = DateTime.Today.AddDays(15)
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Holidays.Should().HaveCount(1);
        result.Holidays[0].Reason.Should().Be("Holiday 1");
    }

    [Fact]
    public async Task Handle_DeleteHoliday_DeletesSuccessfully()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();

        var holiday = new WorkshopHoliday
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            Date = DateTime.Today.AddDays(10),
            Reason = "Holiday to Delete"
        };

        context.WorkshopHolidays.Add(holiday);
        context.SaveChanges();

        var handler = new DeleteHolidayCommandHandler(context);
        var command = new DeleteHolidayCommand
        {
            HolidayId = holiday.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "SuperAdmin" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("deleted");

        var deletedHoliday = context.WorkshopHolidays.FirstOrDefault(h => h.Id == holiday.Id);
        deletedHoliday.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeleteHoliday_NonSuperAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = GetDbContext();
        var workshop = context.Workshops.First();

        var holiday = new WorkshopHoliday
        {
            Id = Guid.NewGuid(),
            WorkshopId = workshop.Id,
            Date = DateTime.Today.AddDays(10),
            Reason = "Holiday"
        };

        context.WorkshopHolidays.Add(holiday);
        context.SaveChanges();

        var handler = new DeleteHolidayCommandHandler(context);
        var command = new DeleteHolidayCommand
        {
            HolidayId = holiday.Id,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new List<string> { "DistributorAdmin" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
