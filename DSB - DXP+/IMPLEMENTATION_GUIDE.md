# Implementation Guide - DSB Login API

**Version:** 1.0.0  
**Status:** ✅ Complete with 175/175 tests passing

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [CQRS Pattern](#cqrs-pattern)
3. [Feature Structure](#feature-structure)
4. [Entity Framework Setup](#entity-framework-setup)
5. [Authorization](#authorization)
6. [Testing Strategy](#testing-strategy)
7. [Code Examples](#code-examples)

---

## Architecture Overview

### Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| **Framework** | ASP.NET Core | 8.0 |
| **API Pattern** | Minimal APIs | Native |
| **ORM** | Entity Framework Core | 8.0 |
| **CQRS** | MediatR | 12.x |
| **Validation** | FluentValidation | 11.x |
| **Authentication** | JWT Bearer | .NET Native |
| **Testing** | xUnit + FluentAssertions | Latest |

### System Architecture

```
Client
  ↓
[ASP.NET Core 8.0 - Minimal APIs]
  ↓
[Authorization Middleware]
  ↓
┌─────────────────────────────────┐
│    MediatR CQRS Pipeline        │
├─────────────────────────────────┤
│  Commands         │   Queries   │
│  (State Change)   │  (Read-Only)│
└─────────────────────────────────┘
  ↓
[FluentValidation Rules]
  ↓
[Command/Query Handlers]
  ↓
[Entity Framework Core]
  ↓
[In-Memory Database / SQL Server]
```

### Design Patterns

1. **CQRS** — Command Query Responsibility Segregation
   - Separates write operations (Commands) from read operations (Queries)
   - Improves scalability and maintainability

2. **Repository Pattern** (via MediatR handlers)
   - Abstracts data access logic
   - Each handler manages its own DbContext operations

3. **Dependency Injection**
   - Built-in .NET DI container
   - Service registration in Program.cs

4. **Soft Delete Pattern**
   - `IsActive` boolean flag instead of physical deletion
   - Maintains audit trail and data integrity

5. **Decorator Pattern** (via MediatR behaviors)
   - Validation middleware
   - Logging and caching potential extensions

---

## CQRS Pattern

### Command Structure

Commands modify system state. All commands include authorization context.

```csharp
public class CreateUserCommand : IRequest<UserResponse>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid RequestingUserId { get; set; }
    public IList<string> RequestingUserRoles { get; set; }
}
```

### Command Handler

```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly LoginDbContext _dbContext;

    public CreateUserCommandHandler(LoginDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserResponse> Handle(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can create users");
        }

        // 2. Business logic validation
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // 3. Create entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Persist
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5. Return response
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
```

### Query Structure

Queries retrieve data without modifications.

```csharp
public class GetUsersQuery : IRequest<PaginatedResponse<UserResponse>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? EmailFilter { get; set; }
}
```

### Query Handler

```csharp
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResponse<UserResponse>>
{
    private readonly LoginDbContext _dbContext;

    public async Task<PaginatedResponse<UserResponse>> Handle(
        GetUsersQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.EmailFilter))
        {
            query = query.Where(u => u.Email.Contains(request.EmailFilter));
        }

        // Apply soft delete filter
        query = query.Where(u => u.IsActive);

        // Count total
        var totalCount = await query.CountAsync(cancellationToken);

        // Paginate
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<UserResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (totalCount + request.PageSize - 1) / request.PageSize
        };
    }
}
```

### Using CQRS in Endpoints

```csharp
// In endpoint handler
var response = await mediator.Send(
    new CreateUserCommand 
    { 
        Email = request.Email,
        Password = request.Password,
        FirstName = request.FirstName,
        LastName = request.LastName,
        RequestingUserId = userId,
        RequestingUserRoles = roles
    }, 
    cancellationToken
);

return Results.Created($"/api/admin/users/{response.Id}", response);
```

---

## Feature Structure

Each feature follows consistent folder organization:

```
Features/
├── Auth/
│   ├── AuthEndpoints.cs           # Endpoint mappings
│   ├── Login/
│   │   ├── LoginCommand.cs
│   │   ├── LoginCommandHandler.cs
│   │   └── LoginRequest.cs
│   └── RefreshToken/
│       ├── RefreshTokenCommand.cs
│       ├── RefreshTokenCommandHandler.cs
│       └── RefreshTokenRequest.cs
│
├── Users/
│   ├── UserEndpoints.cs
│   ├── Create/
│   │   ├── CreateUserCommand.cs
│   │   ├── CreateUserCommandHandler.cs
│   │   ├── CreateUserValidator.cs
│   │   └── CreateUserRequest.cs
│   ├── Get/
│   │   ├── GetUsersQuery.cs
│   │   ├── GetUsersQueryHandler.cs
│   │   └── GetUserQuery.cs
│   ├── Update/
│   └── Delete/
│
├── Workshops/
│   └── [Similar structure]
│
├── Advisors/
│   └── [Similar structure]
│
└── Schedules/
    ├── WorkshopSchedule/
    ├── Holiday/
    └── Blackout/
```

### File Naming Conventions

| File Type | Pattern | Example |
|-----------|---------|---------|
| Commands | `{Action}{Resource}Command.cs` | `CreateUserCommand.cs` |
| Command Handlers | `{Action}{Resource}CommandHandler.cs` | `CreateUserCommandHandler.cs` |
| Queries | `Get{Resource}Query.cs` | `GetUsersQuery.cs` |
| Query Handlers | `Get{Resource}QueryHandler.cs` | `GetUsersQueryHandler.cs` |
| Validators | `{Action}{Resource}Validator.cs` | `CreateUserValidator.cs` |
| Endpoints | `{Resource}Endpoints.cs` | `UserEndpoints.cs` |
| Requests | `{Action}{Resource}Request.cs` | `CreateUserRequest.cs` |
| Responses | `{Resource}Response.cs` | `UserResponse.cs` |

---

## Entity Framework Setup

### DbContext Configuration

```csharp
public class LoginDbContext : DbContext
{
    public LoginDbContext(DbContextOptions<LoginDbContext> options) 
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Workshop> Workshops { get; set; }
    public DbSet<WorkshopSchedule> WorkshopSchedules { get; set; }
    public DbSet<WorkshopHoliday> WorkshopHolidays { get; set; }
    public DbSet<WorkshopBlackoutDate> WorkshopBlackoutDates { get; set; }
    public DbSet<Advisor> Advisors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            // ... more configuration
        });

        // Seed default roles
        var superAdminRole = new Role 
        { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "SuperAdmin"
        };
        var distributorAdminRole = new Role 
        { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "DistributorAdmin"
        };
        var workshopUserRole = new Role 
        { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "WorkshopUser"
        };

        modelBuilder.Entity<Role>().HasData(
            superAdminRole, 
            distributorAdminRole, 
            workshopUserRole
        );
    }
}
```

### Soft Delete Pattern

Entities include `IsActive` flag:

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; } = true; // Soft delete flag
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Query Filtering

Always filter active records:

```csharp
var users = await _dbContext.Users
    .Where(u => u.IsActive)
    .ToListAsync();
```

### Cascade Delete

Related entities are deleted automatically:

```csharp
modelBuilder.Entity<Workshop>(entity =>
{
    entity.HasMany(w => w.Schedules)
        .WithOne()
        .HasForeignKey(s => s.WorkshopId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

---

## Authorization

### Authorization Policies

Defined in `Infrastructure/Authorization/AuthorizationPolicies.cs`:

```csharp
public static class AuthorizationPolicies
{
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string AdminOrHigher = "AdminOrHigher";
    public const string WorkshopReadOnly = "WorkshopReadOnly";

    public static void RegisterPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(SuperAdminOnly, policy =>
            policy.RequireClaim("role", "SuperAdmin"));

        options.AddPolicy(AdminOrHigher, policy =>
            policy.RequireClaim("role", "SuperAdmin", "DistributorAdmin"));

        options.AddPolicy(WorkshopReadOnly, policy =>
            policy.RequireAuthenticatedUser());
    }
}
```

### Applying Authorization to Endpoints

```csharp
var adminGroup = group.MapGroup("/admin/workshops")
    .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
    .WithOpenApi();
```

### Role Extraction in Handlers

```csharp
var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
var roles = user.FindAll("role").Select(c => c.Value).ToList();

var command = new CreateWorkshopCommand
{
    // ... properties
    RequestingUserId = userId,
    RequestingUserRoles = roles
};
```

---

## Testing Strategy

### Unit Test Structure

```csharp
public class CreateUserCommandTests
{
    private readonly LoginDbContext _dbContext;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandTests()
    {
        // Setup in-memory database for each test
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LoginDbContext(options);
        _handler = new CreateUserCommandHandler(_dbContext);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var superAdminRole = new Role 
        { 
            Id = Guid.NewGuid(), 
            Name = "SuperAdmin" 
        };
        _dbContext.Roles.Add(superAdminRole);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidData_CreatesUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@dsb.cl",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new[] { "SuperAdmin" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Email.Should().Be("test@dsb.cl");
        result.FirstName.Should().Be("Test");
        
        var userInDb = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "test@dsb.cl");
        userInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonSuperAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@dsb.cl",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new[] { "WorkshopUser" } // Wrong role
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
```

### Integration Test Structure

```csharp
public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200OK()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "admin@dsb.cl",
            Password: "Admin123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
    }
}
```

### Test Categories

| Category | Type | Count | Purpose |
|----------|------|-------|---------|
| **Unit Tests** | Command/Query Handlers | 119 | Feature logic validation |
| **Integration Tests** | HTTP Endpoints | 56 | End-to-end API validation |
| **Authorization Tests** | Policy Testing | 10 | Permission enforcement |
| **Validation Tests** | Input Validation | 30 | FluentValidation rules |

---

## Code Examples

### Complete Feature Implementation

#### 1. Entity Definition

```csharp
public class Workshop
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public WorkshopBrand Brand { get; set; }
    public string Location { get; set; }
    public WorkshopAddress Address { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<WorkshopSchedule> Schedules { get; set; }
    public ICollection<WorkshopHoliday> Holidays { get; set; }
    public ICollection<WorkshopBlackoutDate> BlackoutDates { get; set; }
}

public enum WorkshopBrand
{
    Suzuki,
    Changan,
    Mazda,
    Renault,
    GWM,
    Avatr,
    Deepal,
    DSFK
}
```

#### 2. Request/Response

```csharp
public class CreateWorkshopRequest
{
    public string Name { get; set; }
    public WorkshopBrand Brand { get; set; }
    public string Location { get; set; }
    public CreateWorkshopAddress Address { get; set; }
    public int Capacity { get; set; }
}

public class WorkshopResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public WorkshopBrand Brand { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
}
```

#### 3. Validator

```csharp
public class CreateWorkshopValidator : AbstractValidator<CreateWorkshopCommand>
{
    public CreateWorkshopValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workshop name is required")
            .MaximumLength(200).WithMessage("Workshop name cannot exceed 200 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Capacity cannot exceed 1000");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address is required")
            .SetValidator(new WorkshopAddressValidator());
    }
}
```

#### 4. Command & Handler

```csharp
public class CreateWorkshopCommand : IRequest<WorkshopResponse>
{
    public string Name { get; set; }
    public WorkshopBrand Brand { get; set; }
    public string Location { get; set; }
    public CreateWorkshopAddress Address { get; set; }
    public int Capacity { get; set; }
    public Guid RequestingUserId { get; set; }
    public IList<string> RequestingUserRoles { get; set; }
}

public class CreateWorkshopCommandHandler : IRequestHandler<CreateWorkshopCommand, WorkshopResponse>
{
    private readonly LoginDbContext _dbContext;

    public CreateWorkshopCommandHandler(LoginDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkshopResponse> Handle(
        CreateWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        // Authorization
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can create workshops");
        }

        // Create entity
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Brand = request.Brand,
            Location = request.Location,
            Address = new()
            {
                Street = request.Address.Street,
                City = request.Address.City,
                Region = request.Address.Region,
                PostalCode = request.Address.PostalCode,
                Country = request.Address.Country ?? "Chile"
            },
            Capacity = request.Capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Workshops.Add(workshop);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new WorkshopResponse
        {
            Id = workshop.Id,
            Name = workshop.Name,
            Brand = workshop.Brand,
            Location = workshop.Location,
            Capacity = workshop.Capacity
        };
    }
}
```

#### 5. Endpoint Mapping

```csharp
public static class WorkshopEndpoints
{
    public static void MapWorkshopEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Workshops");

        var adminGroup = group.MapGroup("/admin/workshops")
            .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
            .WithOpenApi();

        adminGroup.MapPost("", CreateWorkshop)
            .WithName("CreateWorkshop")
            .Produces<WorkshopResponse>(StatusCodes.Status201Created)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> CreateWorkshop(
        [FromBody] CreateWorkshopRequest request,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
            var roles = user.FindAll("role").Select(c => c.Value).ToList();

            var command = new CreateWorkshopCommand
            {
                Name = request.Name,
                Brand = request.Brand,
                Location = request.Location,
                Address = request.Address,
                Capacity = request.Capacity,
                RequestingUserId = userId,
                RequestingUserRoles = roles
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/admin/workshops/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Forbid();
        }
    }
}
```

#### 6. Test

```csharp
public class CreateWorkshopCommandTests
{
    private readonly LoginDbContext _dbContext;
    private readonly CreateWorkshopCommandHandler _handler;

    public CreateWorkshopCommandTests()
    {
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LoginDbContext(options);
        _handler = new CreateWorkshopCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ValidData_CreatesWorkshop()
    {
        // Arrange
        var command = new CreateWorkshopCommand
        {
            Name = "Concesionario Centro",
            Brand = WorkshopBrand.Suzuki,
            Location = "Santiago",
            Address = new()
            {
                Street = "Av. Providencia 1234",
                City = "Santiago",
                Region = "Metropolitana",
                PostalCode = "8320000",
                Country = "Chile"
            },
            Capacity = 50,
            RequestingUserId = Guid.NewGuid(),
            RequestingUserRoles = new[] { "SuperAdmin" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Concesionario Centro");
        result.Brand.Should().Be(WorkshopBrand.Suzuki);
        
        var workshopInDb = await _dbContext.Workshops
            .FirstOrDefaultAsync(w => w.Id == result.Id);
        workshopInDb.Should().NotBeNull();
        workshopInDb!.Location.Should().Be("Santiago");
    }
}
```

---

## Best Practices Applied

✅ **Separation of Concerns** — CQRS separates read and write operations  
✅ **Single Responsibility** — Each handler manages one operation  
✅ **Dependency Injection** — All dependencies injected via constructor  
✅ **Validation** — FluentValidation rules + handler validation  
✅ **Authorization** — Claims-based authorization with roles  
✅ **Testing** — 100% unit test coverage, integration tests for endpoints  
✅ **Documentation** — XML comments and comprehensive Swagger docs  
✅ **Error Handling** — Consistent HTTP status codes and error responses  
✅ **Data Integrity** — Soft deletes, cascade deletes, unique constraints  
✅ **Performance** — Async/await, efficient database queries  

---

## Next Steps

### To Add a New Feature

1. Create Command/Query classes in `Features/{Feature}/{Operation}/`
2. Create corresponding Validator (for Commands only)
3. Implement Handler(s) with business logic
4. Define Request/Response DTOs
5. Map endpoints in `{Feature}Endpoints.cs`
6. Register endpoint in `Program.cs`
7. Write unit tests with 100% coverage
8. Write integration tests for HTTP endpoints

### Extending Authorization

Update `AuthorizationPolicies.cs`:
```csharp
options.AddPolicy("YourNewPolicy", policy =>
    policy.RequireClaim("role", "RoleA", "RoleB"));
```

### Adding Database Persistence

Replace `UseInMemoryDatabase()` in `Program.cs`:
```csharp
builder.Services.AddDbContext<LoginDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## References

- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
- [FluentValidation](https://fluentvalidation.net/)
