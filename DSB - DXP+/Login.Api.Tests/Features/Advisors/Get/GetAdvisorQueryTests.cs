using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Advisors.Get;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Advisors.Get;

public class GetAdvisorQueryTests
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

        var advisor1 = new Advisor
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

        context.Advisors.AddRange(advisor1, advisor2);
        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsAdvisor()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetAdvisorQueryHandler(context);
        var advisor = context.Advisors.First();

        var query = new GetAdvisorQuery { AdvisorId = advisor.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(advisor.Id);
        result.FirstName.Should().Be(advisor.FirstName);
        result.Email.Should().Be(advisor.Email);
    }

    [Fact]
    public async Task Handle_InvalidId_ReturnsNull()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetAdvisorQueryHandler(context);

        var query = new GetAdvisorQuery { AdvisorId = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InactiveAdvisor_ReturnsNull()
    {
        // Arrange
        var context = GetDbContext();
        var advisor = context.Advisors.First();
        advisor.IsActive = false;
        context.SaveChanges();

        var handler = new GetAdvisorQueryHandler(context);
        var query = new GetAdvisorQuery { AdvisorId = advisor.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

public class GetAdvisorsQueryTests
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

        for (int i = 0; i < 5; i++)
        {
            var advisor = new Advisor
            {
                Id = Guid.NewGuid(),
                FirstName = $"Advisor{i}",
                LastName = $"Last{i}",
                Email = $"advisor{i}@dsb.cl",
                PhoneNumber = $"+56912345{i:D3}",
                WorkshopId = workshop.Id,
                AssignedBrand = (AdvisorBrand)(i % 8),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Advisors.Add(advisor);
        }

        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_Page1_ReturnsFirstThreeAdvisors()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetAdvisorsQueryHandler(context);

        var query = new GetAdvisorsQuery { PageNumber = 1, PageSize = 3 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Advisors.Should().HaveCount(3);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithEmailFilter_ReturnsMatchingAdvisors()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetAdvisorsQueryHandler(context);

        var query = new GetAdvisorsQuery { PageNumber = 1, PageSize = 10, EmailFilter = "advisor1" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Advisors.Should().HaveCount(1);
        result.Advisors[0].Email.Should().Contain("advisor1");
    }

    [Fact]
    public async Task Handle_WithBrandFilter_ReturnsMatchingAdvisors()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetAdvisorsQueryHandler(context);

        var query = new GetAdvisorsQuery { PageNumber = 1, PageSize = 10, BrandFilter = AdvisorBrand.Suzuki };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Advisors.Should().HaveCount(1);
        result.Advisors[0].AssignedBrand.Should().Be("Suzuki");
    }

    [Fact]
    public async Task Handle_InactiveAdvisorsExcluded_ReturnsOnlyActive()
    {
        // Arrange
        var context = GetDbContext();
        var advisorToDeactivate = context.Advisors.First();
        advisorToDeactivate.IsActive = false;
        context.SaveChanges();

        var handler = new GetAdvisorsQueryHandler(context);
        var query = new GetAdvisorsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Advisors.Should().HaveCount(4);
        result.Advisors.Should().AllSatisfy(a => a.IsActive.Should().BeTrue());
    }
}
