# Backend Implementation — Login API (`Login.Api`)

> Built on the architecture defined in `backend_blueprint.md`:
> **.NET 10 · ASP.NET Core Web API · Entity Framework Core · SQL Server**
> **Vertical Slice Architecture · MediatR · FluentValidation · BCrypt · JWT**

---

## 1. Project file (`Login.Api.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Login.Api</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- EF Core + SQL Server -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design"    Version="9.*" />

    <!-- Vertical Slice / CQRS -->
    <PackageReference Include="MediatR"                                 Version="12.*" />
    <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.*" />

    <!-- Validation -->
    <PackageReference Include="FluentValidation.AspNetCore"             Version="11.*" />

    <!-- Auth -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.*" />
    <PackageReference Include="BCrypt.Net-Next"                         Version="4.*" />

    <!-- Utilities -->
    <PackageReference Include="Swashbuckle.AspNetCore"                  Version="6.*" />
  </ItemGroup>
</Project>
```

---

## 2. Folder structure

```
Login.Api/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
│
├── Features/
│   ├── Authentication/
│   │   └── Login/
│   │       ├── LoginEndpoint.cs          ← Minimal API endpoint registration
│   │       ├── LoginCommand.cs           ← MediatR IRequest + handler
│   │       ├── LoginRequestDto.cs        ← Inbound payload
│   │       ├── LoginResponseDto.cs       ← Outbound payload
│   │       └── LoginValidator.cs         ← FluentValidation rules
│   │
│   └── Authorization/
│       └── RefreshToken/                 ← (future slice — scaffold only)
│           └── .gitkeep
│
├── Infrastructure/
│   ├── Persistence/
│   │   ├── LoginDbContext.cs             ← EF Core DbContext
│   │   └── Configurations/
│   │       ├── UserConfiguration.cs
│   │       ├── RoleConfiguration.cs
│   │       ├── PermissionConfiguration.cs
│   │       ├── UserRoleConfiguration.cs
│   │       └── RolePermissionConfiguration.cs
│   │
│   └── Security/
│       ├── JwtTokenService.cs            ← JWT generation logic
│       ├── PasswordHasher.cs             ← BCrypt wrapper
│       └── IJwtTokenService.cs
│
├── Domain/
│   └── Entities/
│       ├── User.cs
│       ├── Role.cs
│       ├── Permission.cs
│       ├── UserRole.cs
│       └── RolePermission.cs
│
└── BuildingBlocks/
    ├── Exceptions/
    │   ├── UnauthorizedException.cs
    │   └── ValidationException.cs
    └── Middleware/
        └── GlobalExceptionMiddleware.cs
```

---

## 3. Database schema (`database/migrations/001_initial_tables.sql` — Login tables)

```sql
-- ============================================================
-- LOGIN API — Table Definitions (SQL Server)
-- ============================================================

CREATE TABLE Countries (
    CountryId   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Name        NVARCHAR(100)    NOT NULL,
    Code        VARCHAR(5)       NOT NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Roles (
    RoleId      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    RoleName    NVARCHAR(100)    NOT NULL UNIQUE,
    Description NVARCHAR(255)        NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Permissions (
    PermissionId   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    PermissionName NVARCHAR(100)    NOT NULL UNIQUE,
    Description    NVARCHAR(255)        NULL,
    CreatedAt      DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE RolePermissions (
    RolePermissionId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    RoleId           UNIQUEIDENTIFIER NOT NULL REFERENCES Roles(RoleId)            ON DELETE CASCADE,
    PermissionId     UNIQUEIDENTIFIER NOT NULL REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    CONSTRAINT UQ_RolePermission UNIQUE (RoleId, PermissionId)
);

CREATE TABLE Users (
    UserId        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    FirstName     NVARCHAR(100)    NOT NULL,
    LastName      NVARCHAR(100)    NOT NULL,
    Email         NVARCHAR(150)    NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(MAX)    NOT NULL,
    CountryId     UNIQUEIDENTIFIER     NULL REFERENCES Countries(CountryId),
    WorkshopId    UNIQUEIDENTIFIER     NULL,               -- FK resolved by Management.Api
    Status        BIT              NOT NULL DEFAULT 1,     -- 1 = Active, 0 = Inactive
    Incadea       BIT              NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE UserRoles (
    UserRoleId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    UserId     UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
    RoleId     UNIQUEIDENTIFIER NOT NULL REFERENCES Roles(RoleId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserRole UNIQUE (UserId, RoleId)
);

-- Seed: default roles
INSERT INTO Roles (RoleName, Description) VALUES
    ('Workshop Admin',          'Full access to workshop management'),
    ('Workshop Assistant',      'Limited workshop operations'),
    ('Service Advisor',         'Customer-facing service role'),
    ('Retail Group Admin',      'Multi-workshop group oversight'),
    ('Contact Center Advisor',  'Remote contact operations'),
    ('Branding Consultant',     'Brand management access');
```

---

## 4. Stored procedure (`database/stored_procedures/login/sp_ValidateUserLogin.sql`)

```sql
CREATE OR ALTER PROCEDURE sp_ValidateUserLogin
    @Email    NVARCHAR(150),
    @IpAddress VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.UserId,
        u.FirstName,
        u.LastName,
        u.Email,
        u.PasswordHash,
        u.Status,
        u.Incadea,
        c.Name      AS CountryName,
        r.RoleId,
        r.RoleName
    FROM   Users           u
    LEFT   JOIN Countries  c  ON c.CountryId = u.CountryId
    LEFT   JOIN UserRoles  ur ON ur.UserId   = u.UserId
    LEFT   JOIN Roles      r  ON r.RoleId    = ur.RoleId
    WHERE  u.Email = @Email;
END;
```

> The stored procedure returns the user with its hashed password and role.
> Password verification (`BCrypt.Verify`) is performed in the application layer — **never** inside SQL.

---

## 5. Domain entities (`Domain/Entities/`)

### `User.cs`

```csharp
namespace Login.Api.Domain.Entities;

public class User
{
    public Guid     UserId       { get; set; }
    public string   FirstName    { get; set; } = string.Empty;
    public string   LastName     { get; set; } = string.Empty;
    public string   Email        { get; set; } = string.Empty;
    public string   PasswordHash { get; set; } = string.Empty;
    public Guid?    CountryId    { get; set; }
    public Guid?    WorkshopId   { get; set; }
    public bool     Status       { get; set; }
    public bool     Incadea      { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }

    // Navigation
    public Country?            Country     { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
```

### `Role.cs`

```csharp
namespace Login.Api.Domain.Entities;

public class Role
{
    public Guid   RoleId      { get; set; }
    public string RoleName    { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserRole>        UserRoles        { get; set; } = [];
    public ICollection<RolePermission>  RolePermissions  { get; set; } = [];
}
```

### `Permission.cs`

```csharp
namespace Login.Api.Domain.Entities;

public class Permission
{
    public Guid   PermissionId   { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string? Description   { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
```

### `UserRole.cs`

```csharp
namespace Login.Api.Domain.Entities;

public class UserRole
{
    public Guid UserRoleId { get; set; }
    public Guid UserId     { get; set; }
    public Guid RoleId     { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
```

### `RolePermission.cs`

```csharp
namespace Login.Api.Domain.Entities;

public class RolePermission
{
    public Guid RolePermissionId { get; set; }
    public Guid RoleId           { get; set; }
    public Guid PermissionId     { get; set; }

    public Role       Role       { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
```

---

## 6. EF Core — DbContext (`Infrastructure/Persistence/LoginDbContext.cs`)

```csharp
using Login.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Infrastructure.Persistence;

public class LoginDbContext(DbContextOptions<LoginDbContext> options) : DbContext(options)
{
    public DbSet<User>           Users           => Set<User>();
    public DbSet<Role>           Roles           => Set<Role>();
    public DbSet<Permission>     Permissions     => Set<Permission>();
    public DbSet<UserRole>       UserRoles       => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoginDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

---

## 7. Security layer (`Infrastructure/Security/`)

### `IJwtTokenService.cs`

```csharp
namespace Login.Api.Infrastructure.Security;

public interface IJwtTokenService
{
    string GenerateToken(JwtPayload payload);
}

public record JwtPayload(
    string UserId,
    string Email,
    string FullName,
    string Country,
    string Role
);
```

### `JwtTokenService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Login.Api.Infrastructure.Security;

public class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    public string GenerateToken(JwtPayload payload)
    {
        var secret      = config["Jwt:Secret"]   ?? throw new InvalidOperationException("JWT secret not configured.");
        var issuer      = config["Jwt:Issuer"]   ?? "BookingSystemAuthServer";
        var audience    = config["Jwt:Audience"] ?? "BookingSystemApps";
        var expiryMins  = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   payload.UserId),
            new Claim(JwtRegisteredClaimNames.Email, payload.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim("fullName", payload.FullName),
            new Claim("country",  payload.Country),
            new Claim("role",     payload.Role),
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(expiryMins),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### `PasswordHasher.cs`

```csharp
namespace Login.Api.Infrastructure.Security;

public static class PasswordHasher
{
    // Work factor 12 — strong enough for production, ~300 ms per hash
    public static string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);

    public static bool Verify(string plainPassword, string hash) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, hash);
}
```

---

## 8. Vertical slice — Login feature (`Features/Authentication/Login/`)

### `LoginRequestDto.cs`

```csharp
namespace Login.Api.Features.Authentication.Login;

public record LoginRequestDto(
    string Email,
    string Password
);
```

### `LoginResponseDto.cs`

```csharp
namespace Login.Api.Features.Authentication.Login;

public record LoginResponseDto(
    string Token,
    string Email,
    string FullName,
    string Country,
    string Role,
    int    ExpiresInMinutes
);
```

### `LoginValidator.cs`

```csharp
using FluentValidation;

namespace Login.Api.Features.Authentication.Login;

public class LoginValidator : AbstractValidator<LoginRequestDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
```

### `LoginCommand.cs` (MediatR command + handler)

```csharp
using Login.Api.Infrastructure.Persistence;
using Login.Api.Infrastructure.Security;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Authentication.Login;

// ── Command ────────────────────────────────────────────────────────────────
public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDto>;

// ── Handler ────────────────────────────────────────────────────────────────
public class LoginCommandHandler(
    LoginDbContext    db,
    IJwtTokenService  jwtService,
    IConfiguration    config
) : IRequestHandler<LoginCommand, LoginResponseDto>
{
    public async Task<LoginResponseDto> Handle(
        LoginCommand    request,
        CancellationToken cancellationToken)
    {
        // 1. Execute stored procedure — retrieve user + role by email
        var emailParam = new SqlParameter("@Email", request.Email);

        var results = await db.Database
            .SqlQueryRaw<UserLoginResult>(
                "EXEC sp_ValidateUserLogin @Email",
                emailParam)
            .ToListAsync(cancellationToken);

        var result = results.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        // 2. Verify account status
        if (!result.Status)
            throw new UnauthorizedAccessException("Account is inactive.");

        // 3. Verify password with BCrypt (never in SQL)
        if (!PasswordHasher.Verify(request.Password, result.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        // 4. Generate JWT
        var token = jwtService.GenerateToken(new JwtPayload(
            UserId:   result.UserId.ToString(),
            Email:    result.Email,
            FullName: $"{result.FirstName} {result.LastName}",
            Country:  result.CountryName ?? string.Empty,
            Role:     result.RoleName    ?? string.Empty
        ));

        var expiryMins = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");

        return new LoginResponseDto(
            Token:          token,
            Email:          result.Email,
            FullName:       $"{result.FirstName} {result.LastName}",
            Country:        result.CountryName ?? string.Empty,
            Role:           result.RoleName    ?? string.Empty,
            ExpiresInMinutes: expiryMins
        );
    }
}

// ── Stored procedure result mapping (raw SQL projection) ───────────────────
public class UserLoginResult
{
    public Guid    UserId       { get; set; }
    public string  FirstName    { get; set; } = string.Empty;
    public string  LastName     { get; set; } = string.Empty;
    public string  Email        { get; set; } = string.Empty;
    public string  PasswordHash { get; set; } = string.Empty;
    public bool    Status       { get; set; }
    public bool    Incadea      { get; set; }
    public string? CountryName  { get; set; }
    public Guid?   RoleId       { get; set; }
    public string? RoleName     { get; set; }
}
```

### `LoginEndpoint.cs` (Minimal API)

```csharp
using FluentValidation;
using MediatR;

namespace Login.Api.Features.Authentication.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequestDto              dto,
            IValidator<LoginRequestDto>  validator,
            IMediator                    mediator,
            CancellationToken            ct) =>
        {
            // Validate request
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                var command  = new LoginCommand(dto.Email, dto.Password);
                var response = await mediator.Send(command, ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    title:      "Unauthorized",
                    detail:     ex.Message,
                    statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("Login")
        .WithTags("Authentication")
        .WithSummary("Authenticate user and return JWT token")
        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesValidationProblem()
        .AllowAnonymous();
    }
}
```

---

## 9. Global exception middleware (`BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs`)

```csharp
using System.Net;
using System.Text.Json;

namespace Login.Api.BuildingBlocks.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        (int statusCode, string title) = ex switch
        {
            UnauthorizedAccessException => (401, "Unauthorized"),
            ArgumentException           => (400, "Bad Request"),
            _                           => (500, "Internal Server Error")
        };

        context.Response.StatusCode = statusCode;

        // Never expose stack traces — only title + a safe message
        var body = JsonSerializer.Serialize(new
        {
            title,
            status  = statusCode,
            detail  = statusCode == 500 ? "An unexpected error occurred." : ex.Message
        });

        return context.Response.WriteAsync(body);
    }
}
```

---

## 10. Application entry point (`Program.cs`)

```csharp
using FluentValidation;
using Login.Api.BuildingBlocks.Middleware;
using Login.Api.Features.Authentication.Login;
using Login.Api.Infrastructure.Persistence;
using Login.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LoginDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── MediatR (Vertical Slices) ─────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ── FluentValidation ──────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

// ── JWT Auth ──────────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT:Secret not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ── Security services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(",")
                ?? ["http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ── Swagger ───────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();   // must be first

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

// ── Endpoint registration (one call per slice) ────────────────────────────
app.MapLoginEndpoint();

app.Run();
```

---

## 11. Configuration files

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Secret":        "",
    "Issuer":        "BookingSystemAuthServer",
    "Audience":      "BookingSystemApps",
    "ExpiryMinutes": "60"
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:5173"
  },
  "Logging": {
    "LogLevel": {
      "Default":               "Information",
      "Microsoft.AspNetCore":  "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=BookingSystemDev;User Id=dummy_db_user;Password=dummy_db_password;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret":        "this_is_a_dummy_secret_key_that_must_be_long_enough_32_bytes",
    "Issuer":        "BookingSystemAuthServer",
    "Audience":      "BookingSystemApps",
    "ExpiryMinutes": "60"
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:5173"
  }
}
```

---

## 12. API contract summary

### `POST /api/auth/login`

**Request body:**

```json
{
  "email":    "admin@inchcape.com",
  "password": "Admin1234"
}
```

**Response `200 OK`:**

```json
{
  "token":           "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email":           "admin@inchcape.com",
  "fullName":        "Admin Inchcape",
  "country":         "Colombia",
  "role":            "Workshop Admin",
  "expiresInMinutes": 60
}
```

**Response `401 Unauthorized`:**

```json
{
  "title":  "Unauthorized",
  "status": 401,
  "detail": "Invalid credentials."
}
```

**Response `400 Validation Problem`:**

```json
{
  "errors": {
    "email":    ["Email format is invalid."],
    "password": ["Password is required."]
  }
}
```

---

## 13. JWT payload structure

The generated token decodes to:

```json
{
  "sub":      "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email":    "admin@inchcape.com",
  "fullName": "Admin Inchcape",
  "country":  "Colombia",
  "role":     "Workshop Admin",
  "jti":      "unique-token-id",
  "nbf":      1716825600,
  "exp":      1716829200,
  "iat":      1716825600,
  "iss":      "BookingSystemAuthServer",
  "aud":      "BookingSystemApps"
}
```

---

## 14. Seed script — test user (`database/stored_procedures/login/seed_test_user.sql`)

```sql
-- Seed a test user for development (run once)
-- Password: Admin1234  →  BCrypt hash (workFactor 12)

DECLARE @CountryId UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId    UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleId    UNIQUEIDENTIFIER;

-- Country
INSERT INTO Countries (CountryId, Name, Code)
VALUES (@CountryId, 'Colombia', 'COL');

-- User  (PasswordHash = BCrypt of "Admin1234")
INSERT INTO Users (UserId, FirstName, LastName, Email, PasswordHash, CountryId, Status, Incadea)
VALUES (
    @UserId,
    'Admin',
    'Inchcape',
    'admin@inchcape.com',
    '$2a$12$KIXc3JCdSFhDomelEJG.N.example_bcrypt_hash_replace_before_use',
    @CountryId,
    1,
    0
);

-- Assign role
SELECT @RoleId = RoleId FROM Roles WHERE RoleName = 'Workshop Admin';

INSERT INTO UserRoles (UserRoleId, UserId, RoleId)
VALUES (NEWID(), @UserId, @RoleId);
```

> **Important:** Replace the `PasswordHash` value with a real BCrypt hash generated with workFactor 12 before running.
> Generate it with: `BCrypt.Net.BCrypt.HashPassword("Admin1234", workFactor: 12)`

---

## 15. Connection to frontend (`view-login.md`)

The frontend `auth.service.ts` must be updated to call this real endpoint instead of using the dummy JWT encoder:

```ts
// src/services/auth.service.ts  — updated for real backend

import api from "./api";                           // axios instance (VITE_API_BASE_URL)
import type { LoginPayload } from "@/types/auth.types";

const TOKEN_KEY = "dsb_token";

export async function login(payload: LoginPayload): Promise<void> {
  const { data } = await api.post("/api/auth/login", payload);
  localStorage.setItem(TOKEN_KEY, data.token);
}

export function logout(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function isAuthenticated(): boolean {
  const token = getToken();
  if (!token) return false;
  try {
    const [, b64] = token.split(".");
    const { exp } = JSON.parse(atob(b64));
    return exp > Math.floor(Date.now() / 1000);
  } catch {
    return false;
  }
}
```
