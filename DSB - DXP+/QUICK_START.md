# Quick Start Guide - DSB Login API

**Last Updated:** May 29, 2026  
**Status:** ✅ Ready for Development

---

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- Git
- Postman or similar API client (optional)

---

## Installation & Setup

### 1. Clone Repository

```bash
cd "C:\Users\cesar.reyes\OneDrive - Inchcape\Desktop\DSB-PoC"
cd "DSB - DXP+"
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build Solution

```bash
dotnet build Login.Api
```

**Expected Output:**
```
Build succeeded.
    0 Error(s)
    0 Warning(s)
```

---

## Running the API

### Option 1: Command Line

```bash
dotnet run --project Login.Api
```

**Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### Option 2: Visual Studio

1. Open `DSB.sln`
2. Set `Login.Api` as Startup Project
3. Press `F5` (Debug) or `Ctrl+F5` (Release)

### Option 3: VS Code

```bash
# Ensure you're in DSB - DXP+ directory
cd "DSB - DXP+"

# Run the API
dotnet run --project Login.Api
```

---

## Testing

### Run All Tests

```bash
dotnet test Login.Api.Tests
```

**Expected Output:**
```
Passed!  - Failed:     0, Passed:   175, Skipped:     0, Total:   175
```

### Run Specific Test Category

```bash
# Authentication tests
dotnet test Login.Api.Tests --filter "Category=Authentication"

# User management tests
dotnet test Login.Api.Tests --filter "Features.Users"

# Integration tests only
dotnet test Login.Api.Tests --filter "Integration"
```

### Run Single Test

```bash
dotnet test Login.Api.Tests --filter "Login_WithValidCredentials_Returns200OK"
```

### Generate Coverage Report

```bash
dotnet test Login.Api.Tests /p:CollectCoverage=true
```

---

## API Endpoints

### Access Swagger UI

Once the API is running:

```
https://localhost:5001/swagger
```

Interactive API documentation with "Try it out" functionality.

### Quick Test Flow

1. **Login** to get JWT token:
   ```http
   POST /api/auth/login
   Content-Type: application/json

   {
     "email": "admin@dsb.cl",
     "password": "Admin123!"
   }
   ```

2. **Copy** `accessToken` from response

3. **Use token** in Authorization header:
   ```
   Authorization: Bearer <your_token_here>
   ```

4. **Try endpoints** (all require auth except /login and /refresh):
   - `GET /api/admin/users`
   - `POST /api/admin/workshops`
   - `GET /api/admin/advisors`

---

## Project Structure

```
Login.Api/
├── Features/                        # Feature modules (CQRS)
│   ├── Auth/                       # Authentication
│   │   ├── AuthEndpoints.cs
│   │   ├── Login/
│   │   └── RefreshToken/
│   │
│   ├── Users/                      # User Management
│   │   ├── UserEndpoints.cs
│   │   ├── Create/
│   │   ├── Get/
│   │   ├── Update/
│   │   ├── Delete/
│   │   ├── AssignRole/
│   │   └── RemoveRole/
│   │
│   ├── Workshops/                  # Workshop Management
│   │   ├── WorkshopEndpoints.cs
│   │   ├── Create/
│   │   ├── Get/
│   │   ├── Update/
│   │   └── Delete/
│   │
│   ├── Advisors/                   # Advisor Management
│   │   ├── AdvisorEndpoints.cs
│   │   ├── Create/
│   │   ├── Get/
│   │   ├── Update/
│   │   └── Delete/
│   │
│   └── Schedules/                  # Schedule Management
│       ├── ScheduleEndpoints.cs
│       ├── WorkshopSchedule/       # Weekly hours
│       ├── Holiday/                # Single day closure
│       └── Blackout/               # Date range closure
│
├── Infrastructure/
│   ├── Data/
│   │   ├── LoginDbContext.cs       # EF Core DbContext
│   │   └── Entities/               # Domain models
│   │
│   ├── Authorization/
│   │   └── AuthorizationPolicies.cs
│   │
│   └── Security/
│       └── JwtTokenGenerator.cs
│
├── Program.cs                       # Application entry point
└── Login.Api.csproj               # Project file

Login.Api.Tests/
├── Features/                        # Unit tests by feature
├── Integration/                     # Integration tests
└── Infrastructure/                  # Infrastructure tests
```

---

## Development Workflow

### Adding a New Feature

1. **Create folder** in `Features/{FeatureName}/{Operation}/`
   ```
   Features/Products/Create/
   ├── CreateProductCommand.cs
   ├── CreateProductCommandHandler.cs
   ├── CreateProductValidator.cs
   └── CreateProductRequest.cs
   ```

2. **Implement Command & Handler**
   - Command: What to do (properties + authorization context)
   - Handler: How to do it (business logic)

3. **Add Validation**
   - Create `{Operation}Validator.cs` extending `AbstractValidator<T>`

4. **Create Endpoint**
   - Add method in `{Feature}Endpoints.cs`
   - Map route with `MapGroup()`, `MapPost()`, `MapGet()`, etc.

5. **Register in Program.cs**
   - Add `app.Map{Feature}Endpoints()` in the endpoint registration section

6. **Write Tests**
   - Unit tests: `Login.Api.Tests/Features/{Feature}/{Operation}/Tests.cs`
   - Integration tests: `Login.Api.Tests/Integration/{Feature}Tests.cs`

### Code Example: Adding Product CRUD

**1. Command:**
```csharp
// Features/Products/Create/CreateProductCommand.cs
public class CreateProductCommand : IRequest<ProductResponse>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public Guid RequestingUserId { get; set; }
    public IList<string> RequestingUserRoles { get; set; }
}
```

**2. Handler:**
```csharp
// Features/Products/Create/CreateProductCommandHandler.cs
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly LoginDbContext _dbContext;

    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
            throw new UnauthorizedAccessException("SuperAdmin only");

        var product = new Product { /* ... */ };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProductResponse { /* ... */ };
    }
}
```

**3. Validator:**
```csharp
// Features/Products/Create/CreateProductValidator.cs
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

**4. Endpoint:**
```csharp
// Features/Products/ProductEndpoints.cs
public static void MapProductEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/api/admin/products")
        .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
        .WithOpenApi();

    group.MapPost("", CreateProduct)
        .WithName("CreateProduct")
        .Produces<ProductResponse>(StatusCodes.Status201Created);
}

private static async Task<IResult> CreateProduct(
    [FromBody] CreateProductRequest request,
    ClaimsPrincipal user,
    [FromServices] IMediator mediator,
    CancellationToken cancellationToken)
{
    var userId = Guid.Parse(user.FindFirst("sub")?.Value ?? "");
    var roles = user.FindAll("role").Select(c => c.Value).ToList();

    var command = new CreateProductCommand
    {
        Name = request.Name,
        Price = request.Price,
        RequestingUserId = userId,
        RequestingUserRoles = roles
    };

    var result = await mediator.Send(command, cancellationToken);
    return Results.Created($"/api/admin/products/{result.Id}", result);
}
```

**5. Register in Program.cs:**
```csharp
// After other endpoint mappings
app.MapProductEndpoints();
```

**6. Test:**
```csharp
// Login.Api.Tests/Features/Products/Create/CreateProductCommandTests.cs
[Fact]
public async Task Handle_ValidData_CreatesProduct()
{
    var command = new CreateProductCommand
    {
        Name = "Test Product",
        Price = 99.99m,
        RequestingUserId = Guid.NewGuid(),
        RequestingUserRoles = new[] { "SuperAdmin" }
    };

    var result = await _handler.Handle(command, CancellationToken.None);

    result.Name.Should().Be("Test Product");
    result.Price.Should().Be(99.99m);
}
```

---

## Common Tasks

### Check if API is Running

```bash
curl https://localhost:5001/swagger
```

### Login with Default Admin

```bash
# Request
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@dsb.cl","password":"Admin123!"}'

# Response
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
  "expiresAt": "2026-05-29T22:25:00Z",
  "user": {
    "id": "00000000-0000-0000-0000-000000000001",
    "email": "admin@dsb.cl",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["SuperAdmin"]
  }
}
```

### View Test Results

```bash
# Summary
dotnet test Login.Api.Tests --logger "console;verbosity=minimal"

# Detailed
dotnet test Login.Api.Tests --logger "console;verbosity=normal"

# With stacktraces
dotnet test Login.Api.Tests --logger "console;verbosity=detailed"
```

### Clean Build

```bash
dotnet clean
dotnet build Login.Api
```

### Update NuGet Packages

```bash
# List outdated packages
dotnet list package --outdated

# Update specific package
dotnet add Login.Api package PackageName --version 1.0.0
```

---

## Troubleshooting

### "Port 5001 already in use"

```bash
# Windows - Kill process on port 5001
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:5001 | xargs kill -9
```

### "Tests fail with duplicate endpoint name"

This is already fixed in the codebase. If it occurs:
- Check for duplicate `.WithName()` values in endpoint definitions
- Endpoint names must be globally unique across all endpoints

### "Authorization header not recognized"

- Ensure header format: `Authorization: Bearer <token>`
- Token must be fresh (not expired)
- User must have required role for endpoint

### "Database assertion failures in tests"

In-memory database is recreated for each test. If tests fail:
- Check `SeedDatabase()` methods
- Ensure each test has isolated DbContext
- Verify seed data includes required roles/users

---

## Performance Considerations

### Current Limitations
- In-memory database (suitable for testing/development)
- No caching layer
- No pagination optimization for large datasets

### Production Recommendations
1. Replace in-memory with SQL Server
2. Add Redis caching for frequently accessed data
3. Implement query optimization (indexes, projections)
4. Add rate limiting middleware
5. Implement logging and monitoring
6. Add request validation middleware

---

## Resources

### Documentation Files
- [API_DOCUMENTATION.md](API_DOCUMENTATION.md) — Complete endpoint reference
- [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md) — Architecture & patterns

### External Resources
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net)
- [JWT.io](https://jwt.io)

---

## Support

For issues or questions:
1. Check existing documentation files
2. Review test examples in `Login.Api.Tests/`
3. Check `Program.cs` for configuration patterns
4. Review feature handlers for business logic examples

---

## Next Steps

1. ✅ Run `dotnet build Login.Api` to verify compilation
2. ✅ Run `dotnet test Login.Api.Tests` to verify all tests pass
3. ✅ Run `dotnet run --project Login.Api` to start server
4. ✅ Open `https://localhost:5001/swagger` to explore API
5. 📚 Read `API_DOCUMENTATION.md` for endpoint details
6. 🔨 Start implementing new features following the pattern above

**Happy coding!** 🚀
