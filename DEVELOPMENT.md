# Development Guide

Complete guide for setting up your development environment and understanding the project structure.

## 📋 Table of Contents

- [System Requirements](#system-requirements)
- [Local Setup](#local-setup)
- [Project Structure](#project-structure)
- [Architecture Overview](#architecture-overview)
- [Debugging](#debugging)
- [Common Tasks](#common-tasks)
- [Troubleshooting](#troubleshooting)

---

## System Requirements

### Minimum Requirements

- **OS:** Windows 10+, macOS 10.15+, or Linux (Ubuntu 18.04+)
- **.NET SDK:** 8.0 or higher
- **RAM:** 4GB minimum, 8GB recommended
- **Disk Space:** 5GB for development environment
- **Git:** 2.30 or higher

### Recommended Tools

| Tool | Purpose | Download |
|------|---------|----------|
| Visual Studio Code | Code editor | [Code.visualstudio.com](https://code.visualstudio.com) |
| Visual Studio 2022 Community | Full IDE (optional) | [VisualStudio.microsoft.com](https://visualstudio.microsoft.com) |
| SQL Server 2019+ | Database (optional) | [SQLServer Management Studio](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) |
| Postman | API testing | [Postman.com](https://www.postman.com) |
| Git Bash | Command line (Windows) | [Git-scm.com](https://git-scm.com) |

### VS Code Extensions

- **C# Dev Kit** (Microsoft)
- **REST Client** (Huachao Mao)
- **.NET Extension Pack** (Microsoft)
- **Markdown All in One** (Yu Zhang)

---

## Local Setup

### 1. Clone the Repository

```bash
# Clone with HTTPS
git clone https://github.com/cesar-inchcape/dsb-sdlc_studio-poc.git

# Or clone with SSH (if configured)
git clone git@github.com:cesar-inchcape/dsb-sdlc_studio-poc.git

# Navigate to project
cd dsb-sdlc_studio-poc
```

### 2. Navigate to Project Folder

```bash
cd "DSB - DXP+"
```

### 3. Restore NuGet Packages

```bash
dotnet restore Login.Api
dotnet restore Login.Api.Tests
```

### 4. Build the Solution

```bash
# Build API project
dotnet build Login.Api

# Build test project
dotnet build Login.Api.Tests
```

### 5. Run Tests

```bash
# Run all tests
dotnet test Login.Api.Tests

# Run with output
dotnet test Login.Api.Tests --logger "console;verbosity=detailed"

# Run specific test class
dotnet test Login.Api.Tests --filter "CreateUserCommandTests"
```

### 6. Start Development Server

```bash
# Start API (development mode)
dotnet run --project Login.Api

# Server runs at: https://localhost:5001
# Swagger UI: https://localhost:5001/swagger
```

### 7. Test the API

```bash
# Using curl
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@dsb.cl","password":"Admin123!"}'

# Using PowerShell
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"admin@dsb.cl","password":"Admin123!"}'
```

---

## Project Structure

### Root Directory

```
DSB-PoC/
├── README.md                          ← Project overview
├── CONTRIBUTING.md                    ← Contribution guidelines
├── CODE_OF_CONDUCT.md                 ← Community standards
├── DEVELOPMENT.md                     ← This file
├── .gitignore                         ← Git ignore rules
│
└── DSB - DXP+/                        ← MAIN PROJECT FOLDER
    ├── DSB.sln                        ← Solution file
    ├── QUICK_START.md                 ← Quick setup guide
    ├── API_DOCUMENTATION.md           ← API reference
    ├── IMPLEMENTATION_GUIDE.md        ← Architecture guide
    ├── IMPLEMENTATION_SUMMARY.md      ← Project summary
    │
    ├── Login.Api/                     ← Main API project
    │   ├── Login.Api.csproj
    │   ├── Program.cs                 ← Application entry point
    │   ├── appsettings.json           ← Configuration
    │   │
    │   └── Features/                  ← Vertical slice features
    │       ├── Auth/                  ← Authentication
    │       ├── Users/                 ← User management
    │       ├── Workshops/             ← Workshop management
    │       ├── Advisors/              ← Advisor management
    │       └── Schedules/             ← Schedule management
    │
    ├── Login.Api.Tests/               ← Test project
    │   ├── Login.Api.Tests.csproj
    │   ├── Features/                  ← Feature tests
    │   │   ├── Auth/
    │   │   ├── Users/
    │   │   ├── Workshops/
    │   │   ├── Advisors/
    │   │   └── Schedules/
    │   └── Integration/               ← Integration tests
    │
    ├── Pre_Requirements_Definitions/  ← Original specs
    └── sdlc-studio/                   ← SDLC artifacts
        ├── prd.md                     ← Product requirements
        ├── personas.md                ← User personas
        ├── epics/                     ← Epics/features
        ├── stories/                   ← User stories
        └── plans/                     ← Implementation plans
```

### Feature Folder Structure

Each feature follows vertical slice architecture:

```
Features/FeatureName/
├── FeatureNameEndpoints.cs            ← HTTP routing
├── {Action}{Resource}Command.cs       ← Command definition
├── {Action}{Resource}CommandHandler.cs ← Command logic
├── {Action}{Resource}Validator.cs     ← Input validation
├── Get{Resource}Query.cs              ← Query definition
├── Get{Resource}QueryHandler.cs       ← Query logic
└── Models/                            ← DTOs if needed
    └── {Resource}Dto.cs
```

Example (User Feature):
```
Features/Users/
├── UserEndpoints.cs
├── Create/
│   ├── CreateUserCommand.cs
│   ├── CreateUserCommandHandler.cs
│   ├── CreateUserValidator.cs
│   └── CreateUserResponse.cs
├── Update/
│   ├── UpdateUserCommand.cs
│   ├── UpdateUserCommandHandler.cs
│   └── UpdateUserValidator.cs
├── Get/
│   ├── GetUserQuery.cs
│   ├── GetUserQueryHandler.cs
│   ├── GetUsersQuery.cs
│   ├── GetUsersQueryHandler.cs
│   ├── GetUsersResponse.cs
│   └── UserDto.cs
└── Delete/
    ├── DeleteUserCommand.cs
    └── DeleteUserCommandHandler.cs
```

---

## Architecture Overview

### CQRS Pattern (Command Query Responsibility Segregation)

```
HTTP Request
    ↓
Endpoints.cs (routing)
    ↓
├─→ Commands (Write)
│   ├→ CommandValidator (validate input)
│   └→ CommandHandler (execute, return result)
│
└─→ Queries (Read)
    ├→ Query (parameters)
    └→ QueryHandler (retrieve, return result)
    ↓
HTTP Response (200 OK, 400 Bad Request, 401 Unauthorized, etc.)
```

### Data Flow

```
1. HTTP Request arrives at endpoint
   └→ Mapped to Command/Query object

2. Validation Pipeline
   └→ FluentValidation rules checked

3. Handler Execution
   └→ Database operations (EF Core)
   └→ Business logic
   └→ Authorization checks

4. Response Serialization
   └→ JSON response

5. HTTP Response sent to client
```

### Authorization Flow

```
Request
  ↓
Check JWT Token
  ├→ Valid? Continue
  ├→ Expired? Return 401
  └→ Invalid? Return 401
  ↓
Extract Claims (including role)
  ↓
Authorization Policy Check
  ├→ SuperAdminOnly
  ├→ AdminOrHigher
  └→ WorkshopReadOnly
  ↓
Handler-level Authorization (additional checks)
  ↓
Execute business logic
```

### Database Layer

```
Entity Framework Core (In-Memory for tests, SQL Server for production)
    ↓
LoginDbContext
    ├→ DbSet<User>
    ├→ DbSet<Role>
    ├→ DbSet<UserRole>
    ├→ DbSet<RefreshToken>
    ├→ DbSet<Workshop>
    ├→ DbSet<Advisor>
    ├→ DbSet<WorkshopSchedule>
    ├→ DbSet<WorkshopHoliday>
    └→ DbSet<WorkshopBlackoutDate>
```

---

## Debugging

### Visual Studio Code

1. **Install C# Dev Kit** extension
2. Open `.vscode/launch.json` (or create it):
   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": "Login.Api",
         "type": "coreclr",
         "request": "launch",
         "preLaunchTask": "build",
         "program": "${workspaceFolder}/DSB - DXP+/Login.Api/bin/Debug/net8.0/Login.Api.dll",
         "args": [],
         "stopAtEntry": false,
         "serverReadyAction": {
           "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
           "uriFormat": "{0}",
           "action": "openExternally"
         }
       }
     ]
   }
   ```
3. Press `F5` to start debugging
4. Set breakpoints by clicking line numbers
5. Use Debug Console to inspect variables

### Debugging Tests

```bash
# Run single test with verbose output
dotnet test Login.Api.Tests --filter "CreateUserCommandTests.CreateUser_WithValidEmail_ReturnsNewUser" -v detailed

# Run with debugger (VS Code)
# 1. Set breakpoint in test file
# 2. Press F5 to debug
# 3. Or use Test Explorer in VS Code
```

### Common Breakpoint Patterns

```csharp
// In CommandHandler.cs
public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    // Breakpoint here to inspect request
    var validator = new CreateUserValidator();
    var validationResult = await validator.ValidateAsync(request);
    
    // Breakpoint to see if validation passed
    if (!validationResult.IsValid)
    {
        throw new ValidationException(validationResult.Errors);
    }
    
    // Breakpoint before saving
    var newUser = new User { Email = request.Email, ... };
    _context.Users.Add(newUser);
    
    // Breakpoint after save to see ID assigned
    await _context.SaveChangesAsync(cancellationToken);
    
    return newUser;
}
```

---

## Common Tasks

### Add a New Feature

1. **Create feature folder:**
   ```bash
   mkdir "Features/YourFeature"
   mkdir "Features/YourFeature/Create"
   mkdir "Features/YourFeature/Get"
   mkdir "Features/YourFeature/Update"
   mkdir "Features/YourFeature/Delete"
   ```

2. **Create Command and Handler:**
   ```csharp
   // Features/YourFeature/Create/CreateYourFeatureCommand.cs
   public class CreateYourFeatureCommand : IRequest<YourFeatureResponse>
   {
       public string Name { get; set; }
       // ... properties
   }
   ```

3. **Create Validator:**
   ```csharp
   // Features/YourFeature/Create/CreateYourFeatureValidator.cs
   public class CreateYourFeatureValidator : AbstractValidator<CreateYourFeatureCommand>
   {
       public CreateYourFeatureValidator()
       {
           RuleFor(x => x.Name)
               .NotEmpty()
               .MaximumLength(200);
       }
   }
   ```

4. **Create Handler:**
   ```csharp
   // Features/YourFeature/Create/CreateYourFeatureCommandHandler.cs
   public class CreateYourFeatureCommandHandler : IRequestHandler<CreateYourFeatureCommand, YourFeatureResponse>
   {
       private readonly LoginDbContext _context;
       
       public CreateYourFeatureCommandHandler(LoginDbContext context)
       {
           _context = context;
       }
       
       public async Task<YourFeatureResponse> Handle(CreateYourFeatureCommand request, CancellationToken cancellationToken)
       {
           // Implementation
       }
   }
   ```

5. **Create Endpoints:**
   ```csharp
   // Features/YourFeature/YourFeatureEndpoints.cs
   public static void MapYourFeatureEndpoints(this WebApplication app)
   {
       var group = app.MapGroup("/api/admin/your-feature")
           .WithTags("YourFeature")
           .RequireAuthorization("SuperAdminOnly");
       
       group.MapPost("", CreateYourFeature)
           .WithName("CreateYourFeature")
           .WithOpenApi();
   }
   
   private static async Task<IResult> CreateYourFeature(
       CreateYourFeatureCommand command,
       IMediator mediator)
   {
       var result = await mediator.Send(command);
       return Results.Created($"/api/admin/your-feature/{result.Id}", result);
   }
   ```

6. **Create Tests:**
   ```csharp
   // Login.Api.Tests/Features/YourFeature/Create/CreateYourFeatureCommandTests.cs
   public class CreateYourFeatureCommandTests
   {
       [Fact]
       public async Task Handle_WithValidData_CreatesAndReturnsYourFeature()
       {
           // Arrange
           var context = new InMemoryDbContextFactory().CreateDbContext();
           var handler = new CreateYourFeatureCommandHandler(context);
           var command = new CreateYourFeatureCommand { Name = "Test" };
           
           // Act
           var result = await handler.Handle(command, CancellationToken.None);
           
           // Assert
           result.Should().NotBeNull();
           result.Name.Should().Be("Test");
       }
   }
   ```

7. **Register in Program.cs:**
   ```csharp
   // Add to MapEndpoints section
   app.MapYourFeatureEndpoints();
   ```

8. **Run and test:**
   ```bash
   dotnet build Login.Api
   dotnet test Login.Api.Tests
   dotnet run --project Login.Api
   ```

### Modify Existing Entity

If you need to add fields to existing entities:

1. **Update the entity class:**
   ```csharp
   // Infrastructure/Data/Entities/User.cs
   public class User
   {
       public Guid Id { get; set; }
       public string Email { get; set; }
       // ... existing properties
       
       // New property
       public DateTime LastLoginAt { get; set; }
   }
   ```

2. **Update related validators if needed**

3. **Update tests:**
   ```bash
   dotnet test Login.Api.Tests
   ```

4. **Create migration (when moving to SQL Server):**
   ```bash
   dotnet ef migrations add AddLastLoginAtToUser --project Login.Api
   ```

### Run Specific Test Category

```bash
# Authentication tests
dotnet test Login.Api.Tests --filter "Category=Authentication"

# User management tests
dotnet test Login.Api.Tests --filter "ClassName~User"

# Only integration tests
dotnet test Login.Api.Tests --filter "FullyQualifiedName~Integration"

# Only unit tests
dotnet test Login.Api.Tests --filter "FullyQualifiedName~Features"
```

### View Test Coverage

```bash
# Generate coverage report
dotnet test Login.Api.Tests \
  --collect:"XPlat Code Coverage" \
  --results-directory:coverage

# View in VS Code
# Install Coverage Gutters extension
# Open coverage file to see colored coverage
```

---

## Troubleshooting

### Port 5001 Already in Use

```bash
# Check what's using port 5001
netstat -ano | findstr :5001

# Kill the process (replace PID)
taskkill /PID <PID> /F

# Or change port in launchSettings.json
```

### Tests Failing with "Duplicate endpoint name"

This error occurs when multiple endpoints have the same name:

```bash
# Solution: Ensure each endpoint.WithName() has unique name
# Check AuthEndpoints.cs and other endpoint files
```

### Build Errors

```bash
# Clean build
dotnet clean Login.Api
dotnet clean Login.Api.Tests
dotnet build Login.Api
dotnet build Login.Api.Tests

# Restore packages if needed
dotnet nuget locals all --clear
dotnet restore Login.Api
dotnet restore Login.Api.Tests
```

### Test Timeout Issues

```bash
# Run with longer timeout
dotnet test Login.Api.Tests --logger "console;verbosity=minimal" --no-build

# Or run specific test
dotnet test Login.Api.Tests --filter "TestName" -v detailed
```

### Database Connection Issues

For development, the project uses in-memory database. No connection needed. If switching to SQL Server:

1. **Create database:**
   ```sql
   CREATE DATABASE DSB
   ```

2. **Update appsettings.json:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=DSB;Trusted_Connection=true;"
     }
   }
   ```

3. **Apply migrations:**
   ```bash
   dotnet ef database update --project Login.Api
   ```

### Hot Reload Not Working

```bash
# Use watch mode instead
dotnet watch run --project Login.Api

# This rebuilds and restarts when files change
```

---

## Resources

- **[QUICK_START.md](DSB%20-%20DXP%2B/QUICK_START.md)** — Fast setup guide
- **[IMPLEMENTATION_GUIDE.md](DSB%20-%20DXP%2B/IMPLEMENTATION_GUIDE.md)** — Architecture details
- **[API_DOCUMENTATION.md](DSB%20-%20DXP%2B/API_DOCUMENTATION.md)** — Endpoint reference
- **[Microsoft C# Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/)** — Language reference
- **[MediatR Documentation](https://github.com/jbogard/MediatR)** — CQRS library
- **[FluentValidation](https://fluentvalidation.net/)** — Validation library
- **[Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)** — ORM documentation

---

**Questions?** Open an issue or check the [CONTRIBUTING.md](CONTRIBUTING.md) guide!
