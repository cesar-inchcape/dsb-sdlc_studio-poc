using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Login.Api.Features.Workshops.Get;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Tests.Features.Workshops.Get;

public class GetWorkshopQueryTests
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
        var workshops = new List<Workshop>
        {
            new Workshop
            {
                Id = Guid.NewGuid(),
                Name = "Suzuki Downtown",
                Brand = WorkshopBrand.Suzuki,
                Location = "Downtown Santiago",
                Capacity = 50,
                Address = new Address { Street = "Av. Alameda 1000", City = "Santiago", Region = "Metropolitan", PostalCode = "8320000", Country = "Chile" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Workshop
            {
                Id = Guid.NewGuid(),
                Name = "Suzuki South",
                Brand = WorkshopBrand.Suzuki,
                Location = "South Santiago",
                Capacity = 40,
                Address = new Address { Street = "Street 1", City = "Santiago", Region = "Metropolitan", PostalCode = "8320000", Country = "Chile" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Workshop
            {
                Id = Guid.NewGuid(),
                Name = "Changan Downtown",
                Brand = WorkshopBrand.Changan,
                Location = "Downtown Santiago",
                Capacity = 60,
                Address = new Address { Street = "Av. Alameda 2000", City = "Santiago", Region = "Metropolitan", PostalCode = "8320000", Country = "Chile" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Workshop
            {
                Id = Guid.NewGuid(),
                Name = "Inactive Workshop",
                Brand = WorkshopBrand.Mazda,
                Location = "Valparaiso",
                Capacity = 30,
                Address = new Address { Street = "Street 2", City = "Valparaiso", Region = "Valparaiso", PostalCode = "2340000", Country = "Chile" },
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Workshops.AddRange(workshops);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetWorkshop_WithValidId_ReturnsWorkshop()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopQueryHandler(context);
        var workshop = context.Workshops.First(w => w.IsActive);

        var query = new GetWorkshopQuery { WorkshopId = workshop.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(workshop.Id);
        result.Name.Should().Be(workshop.Name);
        result.Brand.Should().Be(workshop.Brand.ToString());
    }

    [Fact]
    public async Task GetWorkshop_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopQueryHandler(context);

        var query = new GetWorkshopQuery { WorkshopId = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkshop_WithInactiveWorkshop_ReturnsNull()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopQueryHandler(context);
        var inactiveWorkshop = context.Workshops.First(w => !w.IsActive);

        var query = new GetWorkshopQuery { WorkshopId = inactiveWorkshop.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkshops_WithoutFilters_ReturnsAllActive()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopsQueryHandler(context);

        var query = new GetWorkshopsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            BrandFilter = null
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Workshops.Should().HaveCount(3); // Only active workshops
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetWorkshops_WithBrandFilter_ReturnsBrandWorkshops()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopsQueryHandler(context);

        var query = new GetWorkshopsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            BrandFilter = WorkshopBrand.Suzuki
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Workshops.Should().HaveCount(2);
        result.Workshops.Should().AllSatisfy(w => w.Brand.Should().Be("Suzuki"));
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkshops_WithLocationFilter_ReturnsMatchingWorkshops()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopsQueryHandler(context);

        var query = new GetWorkshopsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            BrandFilter = null,
            LocationFilter = "downtown"
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Workshops.Should().HaveCount(2);
        result.Workshops.Should().AllSatisfy(w => w.Location.Should().Contain("Downtown"));
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkshops_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopsQueryHandler(context);

        // Page 1 with size 2
        var query1 = new GetWorkshopsQuery
        {
            PageNumber = 1,
            PageSize = 2,
            BrandFilter = null
        };

        // Act
        var result1 = await handler.Handle(query1, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result1.Workshops.Should().HaveCount(2);
        result1.TotalCount.Should().Be(3);
        result1.PageNumber.Should().Be(1);
        result1.PageSize.Should().Be(2);
        result1.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkshops_WithPage2_ReturnsRemainingResults()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopsQueryHandler(context);

        // Page 2 with size 2
        var query2 = new GetWorkshopsQuery
        {
            PageNumber = 2,
            PageSize = 2,
            BrandFilter = null
        };

        // Act
        var result2 = await handler.Handle(query2, CancellationToken.None);

        // Assert
        result2.Should().NotBeNull();
        result2.Workshops.Should().HaveCount(1);
        result2.TotalCount.Should().Be(3);
        result2.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkshops_CombinedFilters_ReturnsFilteredResults()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopsQueryHandler(context);

        var query = new GetWorkshopsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            BrandFilter = WorkshopBrand.Suzuki,
            LocationFilter = "south"
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Workshops.Should().HaveCount(1);
        result.Workshops[0].Name.Should().Be("Suzuki South");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetWorkshops_NoMatchingFilters_ReturnsEmpty()
    {
        // Arrange
        var context = GetDbContext();
        var handler = new GetWorkshopsQueryHandler(context);

        var query = new GetWorkshopsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            BrandFilter = WorkshopBrand.Avatr, // Brand with no workshops
            LocationFilter = null
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Workshops.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
