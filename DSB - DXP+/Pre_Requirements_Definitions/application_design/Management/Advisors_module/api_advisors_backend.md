# Backend Implementation — Advisors API (`Management.Api`)

> Built on the architecture defined in `backend_blueprint.md`:
> **.NET 10 · ASP.NET Core Web API · Entity Framework Core · SQL Server**
> **Vertical Slice Architecture · MediatR · FluentValidation · JWT Auth**
>
> Advisors belong to the **Management API** module (`src/Services/Management.Api/Features/Advisors/`).

---

## 1. NuGet packages — `Management.Api.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Management.Api</RootNamespace>
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

    <!-- Auth (token validation from Login.Api) -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.*" />

    <!-- Utilities -->
    <PackageReference Include="Swashbuckle.AspNetCore"                  Version="6.*" />
  </ItemGroup>
</Project>
```

---

## 2. Folder structure — Advisors feature

```
Management.Api/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
│
├── Features/
│   └── Advisors/
│       ├── GetAdvisors/
│       │   ├── GetAdvisorsEndpoint.cs
│       │   ├── GetAdvisorsQuery.cs          ← IRequest + Handler (SP)
│       │   ├── GetAdvisorsRequestDto.cs     ← filters + pagination params
│       │   └── GetAdvisorsResponseDto.cs
│       │
│       ├── GetAdvisorById/
│       │   ├── GetAdvisorByIdEndpoint.cs
│       │   ├── GetAdvisorByIdQuery.cs       ← IRequest + Handler (SP)
│       │   └── AdvisorDetailResponseDto.cs
│       │
│       ├── CreateAdvisor/
│       │   ├── CreateAdvisorEndpoint.cs
│       │   ├── CreateAdvisorCommand.cs      ← IRequest + Handler (EF Core)
│       │   ├── CreateAdvisorRequestDto.cs
│       │   ├── CreateAdvisorResponseDto.cs
│       │   └── CreateAdvisorValidator.cs
│       │
│       ├── UpdateAdvisor/
│       │   ├── UpdateAdvisorEndpoint.cs
│       │   ├── UpdateAdvisorCommand.cs      ← IRequest + Handler (EF Core)
│       │   ├── UpdateAdvisorRequestDto.cs
│       │   ├── UpdateAdvisorResponseDto.cs
│       │   └── UpdateAdvisorValidator.cs
│       │
│       ├── ToggleAdvisorStatus/
│       │   ├── ToggleAdvisorStatusEndpoint.cs
│       │   ├── ToggleAdvisorStatusCommand.cs ← IRequest + Handler (EF Core)
│       │   ├── ToggleAdvisorStatusRequestDto.cs
│       │   └── ToggleAdvisorStatusValidator.cs
│       │
│       └── DeleteAdvisor/
│           ├── DeleteAdvisorEndpoint.cs
│           └── DeleteAdvisorCommand.cs      ← IRequest + Handler (SP — transactional)
│
├── Infrastructure/
│   └── Persistence/
│       ├── ManagementDbContext.cs
│       └── Configurations/
│           ├── AdvisorConfiguration.cs
│           ├── BrandConfiguration.cs
│           ├── AdvisorBrandConfiguration.cs
│           ├── ServiceTypeConfiguration.cs
│           └── AdvisorServiceTypeConfiguration.cs
│
├── Domain/
│   └── Entities/
│       ├── Advisor.cs
│       ├── Workshop.cs           ← read-only reference (owned by Workshops sub-module)
│       ├── Brand.cs
│       ├── AdvisorBrand.cs
│       ├── ServiceType.cs
│       └── AdvisorServiceType.cs
│
└── BuildingBlocks/
    ├── Exceptions/
    │   ├── NotFoundException.cs
    │   └── ValidationException.cs
    └── Middleware/
        └── GlobalExceptionMiddleware.cs
```

---

## 3. Database schema (`database/migrations/001_initial_tables.sql` — Management tables)

```sql
-- ============================================================
-- MANAGEMENT API — Advisors Domain Tables (SQL Server)
-- ============================================================

-- Reference table (owned by Workshops sub-module, shown here for FK context)
CREATE TABLE Workshops (
    WorkshopId   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Name         NVARCHAR(150)    NOT NULL,
    CityId       UNIQUEIDENTIFIER     NULL,
    Status       BIT              NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Brand (
    BrandId   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Name      NVARCHAR(100)    NOT NULL UNIQUE,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE ServiceTypes (
    ServiceTypeId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Name          NVARCHAR(100)    NOT NULL UNIQUE,
    CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Advisors (
    AdvisorId    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    FirstName    NVARCHAR(100)    NOT NULL,
    LastName     NVARCHAR(100)    NOT NULL,
    Phone        VARCHAR(20)          NULL,
    WorkshopId   UNIQUEIDENTIFIER     NULL REFERENCES Workshops(WorkshopId),
    Status       BIT              NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

-- Advisor ↔ Brand  (many-to-many)
CREATE TABLE AdvisorBrand (
    AdvisorBrandId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    AdvisorId      UNIQUEIDENTIFIER NOT NULL REFERENCES Advisors(AdvisorId)  ON DELETE CASCADE,
    BrandId        UNIQUEIDENTIFIER NOT NULL REFERENCES Brand(BrandId)       ON DELETE CASCADE,
    CONSTRAINT UQ_AdvisorBrand UNIQUE (AdvisorId, BrandId)
);

-- Advisor ↔ ServiceType  (many-to-many)
CREATE TABLE AdvisorServiceType (
    AdvisorServiceTypeId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    AdvisorId            UNIQUEIDENTIFIER NOT NULL REFERENCES Advisors(AdvisorId)     ON DELETE CASCADE,
    ServiceTypeId        UNIQUEIDENTIFIER NOT NULL REFERENCES ServiceTypes(ServiceTypeId) ON DELETE CASCADE,
    CONSTRAINT UQ_AdvisorServiceType UNIQUE (AdvisorId, ServiceTypeId)
);

-- ── Seed: Brands ─────────────────────────────────────────────────────────
INSERT INTO Brand (Name) VALUES
    ('HINO'), ('Subaru'), ('BMW'), ('DFSK'), ('Mercedes-Benz'), ('Toyota');

-- ── Seed: Service types ───────────────────────────────────────────────────
INSERT INTO ServiceTypes (Name) VALUES
    ('Mechanical repairs'),
    ('Preventive maintenance'),
    ('Dent removal and painting'),
    ('Express'),
    ('Recalls');

-- ── Seed: Workshops ───────────────────────────────────────────────────────
INSERT INTO Workshops (Name) VALUES
    ('Industrias Mussgo Bogotá'),
    ('Proautos Barranquilla'),
    ('STK Power'),
    ('Rasautos Manizales'),
    ('Carrazos'),
    ('Seikou Bogotá'),
    ('Uno A Automotriz'),
    ('Motor Hino / MotorK DFSK');
```

---

## 4. Stored procedures (`database/stored_procedures/management/`)

### `sp_GetAdvisors.sql`

```sql
CREATE OR ALTER PROCEDURE sp_GetAdvisors
    @NameFilter        NVARCHAR(100) = NULL,
    @WorkshopFilter    NVARCHAR(150) = NULL,
    @BrandFilter       NVARCHAR(100) = NULL,
    @ServiceFilter     NVARCHAR(100) = NULL,
    @PageNumber        INT           = 1,
    @PageSize          INT           = 10,
    @TotalCount        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Aggregated brands and services per advisor
    WITH AdvisorBrands AS (
        SELECT
            ab.AdvisorId,
            STRING_AGG(b.Name, ', ') WITHIN GROUP (ORDER BY b.Name) AS Brands
        FROM AdvisorBrand ab
        JOIN Brand b ON b.BrandId = ab.BrandId
        GROUP BY ab.AdvisorId
    ),
    AdvisorServices AS (
        SELECT
            ast.AdvisorId,
            STRING_AGG(st.Name, ', ') WITHIN GROUP (ORDER BY st.Name) AS Services
        FROM AdvisorServiceType ast
        JOIN ServiceTypes st ON st.ServiceTypeId = ast.ServiceTypeId
        GROUP BY ast.AdvisorId
    ),
    Filtered AS (
        SELECT
            a.AdvisorId,
            a.FirstName,
            a.LastName,
            CONCAT(a.FirstName, ' ', a.LastName) AS FullName,
            a.Phone,
            a.Status,
            a.CreatedAt,
            a.UpdatedAt,
            w.WorkshopId,
            w.Name          AS WorkshopName,
            ab.Brands,
            ast.Services
        FROM   Advisors     a
        LEFT   JOIN Workshops        w   ON w.WorkshopId  = a.WorkshopId
        LEFT   JOIN AdvisorBrands    ab  ON ab.AdvisorId  = a.AdvisorId
        LEFT   JOIN AdvisorServices  ast ON ast.AdvisorId = a.AdvisorId
        WHERE
            (@NameFilter     IS NULL OR CONCAT(a.FirstName,' ',a.LastName) LIKE '%' + @NameFilter  + '%')
            AND (@WorkshopFilter IS NULL OR w.Name    LIKE '%' + @WorkshopFilter + '%')
            AND (@BrandFilter    IS NULL OR ab.Brands LIKE '%' + @BrandFilter    + '%')
            AND (@ServiceFilter  IS NULL OR ast.Services LIKE '%' + @ServiceFilter + '%')
    )
    SELECT @TotalCount = COUNT(*) FROM Filtered;

    WITH AdvisorBrands AS (
        SELECT ab.AdvisorId, STRING_AGG(b.Name, ', ') WITHIN GROUP (ORDER BY b.Name) AS Brands
        FROM AdvisorBrand ab JOIN Brand b ON b.BrandId = ab.BrandId GROUP BY ab.AdvisorId
    ),
    AdvisorServices AS (
        SELECT ast.AdvisorId, STRING_AGG(st.Name, ', ') WITHIN GROUP (ORDER BY st.Name) AS Services
        FROM AdvisorServiceType ast JOIN ServiceTypes st ON st.ServiceTypeId = ast.ServiceTypeId GROUP BY ast.AdvisorId
    )
    SELECT
        a.AdvisorId,
        CONCAT(a.FirstName, ' ', a.LastName) AS FullName,
        a.Phone,
        a.Status,
        w.WorkshopId,
        w.Name   AS WorkshopName,
        ab.Brands,
        ast.Services
    FROM   Advisors a
    LEFT   JOIN Workshops       w   ON w.WorkshopId  = a.WorkshopId
    LEFT   JOIN AdvisorBrands   ab  ON ab.AdvisorId  = a.AdvisorId
    LEFT   JOIN AdvisorServices ast ON ast.AdvisorId = a.AdvisorId
    WHERE
        (@NameFilter     IS NULL OR CONCAT(a.FirstName,' ',a.LastName) LIKE '%' + @NameFilter  + '%')
        AND (@WorkshopFilter IS NULL OR w.Name    LIKE '%' + @WorkshopFilter + '%')
        AND (@BrandFilter    IS NULL OR ab.Brands LIKE '%' + @BrandFilter    + '%')
        AND (@ServiceFilter  IS NULL OR ast.Services LIKE '%' + @ServiceFilter + '%')
    ORDER BY a.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
```

### `sp_GetAdvisorById.sql`

```sql
CREATE OR ALTER PROCEDURE sp_GetAdvisorById
    @AdvisorId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Main advisor row
    SELECT
        a.AdvisorId,
        a.FirstName,
        a.LastName,
        a.Phone,
        a.Status,
        w.WorkshopId,
        w.Name AS WorkshopName
    FROM Advisors a
    LEFT JOIN Workshops w ON w.WorkshopId = a.WorkshopId
    WHERE a.AdvisorId = @AdvisorId;

    -- Brands (second result set)
    SELECT b.BrandId, b.Name AS BrandName
    FROM AdvisorBrand ab
    JOIN Brand b ON b.BrandId = ab.BrandId
    WHERE ab.AdvisorId = @AdvisorId;

    -- Services (third result set)
    SELECT st.ServiceTypeId, st.Name AS ServiceName
    FROM AdvisorServiceType ast
    JOIN ServiceTypes st ON st.ServiceTypeId = ast.ServiceTypeId
    WHERE ast.AdvisorId = @AdvisorId;
END;
```

### `sp_DeleteAdvisor.sql`

```sql
CREATE OR ALTER PROCEDURE sp_DeleteAdvisor
    @AdvisorId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- auto-rollback on any error

    BEGIN TRANSACTION;

        -- Remove junction records first (FK safety, CASCADE handles it but explicit is cleaner)
        DELETE FROM AdvisorBrand       WHERE AdvisorId = @AdvisorId;
        DELETE FROM AdvisorServiceType WHERE AdvisorId = @AdvisorId;

        -- Remove the advisor
        DELETE FROM Advisors WHERE AdvisorId = @AdvisorId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK;
            RAISERROR('Advisor not found.', 16, 1);
            RETURN;
        END

    COMMIT;
END;
```

---

## 5. Domain entities (`Domain/Entities/`)

### `Advisor.cs`

```csharp
namespace Management.Api.Domain.Entities;

public class Advisor
{
    public Guid     AdvisorId  { get; set; }
    public string   FirstName  { get; set; } = string.Empty;
    public string   LastName   { get; set; } = string.Empty;
    public string?  Phone      { get; set; }
    public Guid?    WorkshopId { get; set; }
    public bool     Status     { get; set; } = true;
    public DateTime CreatedAt  { get; set; }
    public DateTime UpdatedAt  { get; set; }

    // Navigation
    public Workshop?                      Workshop     { get; set; }
    public ICollection<AdvisorBrand>      AdvisorBrands       { get; set; } = [];
    public ICollection<AdvisorServiceType> AdvisorServiceTypes { get; set; } = [];
}
```

### `Brand.cs`

```csharp
namespace Management.Api.Domain.Entities;

public class Brand
{
    public Guid   BrandId   { get; set; }
    public string Name      { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<AdvisorBrand> AdvisorBrands { get; set; } = [];
}
```

### `AdvisorBrand.cs`

```csharp
namespace Management.Api.Domain.Entities;

public class AdvisorBrand
{
    public Guid AdvisorBrandId { get; set; }
    public Guid AdvisorId      { get; set; }
    public Guid BrandId        { get; set; }

    public Advisor Advisor { get; set; } = null!;
    public Brand   Brand   { get; set; } = null!;
}
```

### `ServiceType.cs`

```csharp
namespace Management.Api.Domain.Entities;

public class ServiceType
{
    public Guid   ServiceTypeId { get; set; }
    public string Name          { get; set; } = string.Empty;
    public DateTime CreatedAt   { get; set; }

    public ICollection<AdvisorServiceType> AdvisorServiceTypes { get; set; } = [];
}
```

### `AdvisorServiceType.cs`

```csharp
namespace Management.Api.Domain.Entities;

public class AdvisorServiceType
{
    public Guid AdvisorServiceTypeId { get; set; }
    public Guid AdvisorId            { get; set; }
    public Guid ServiceTypeId        { get; set; }

    public Advisor     Advisor     { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
}
```

---

## 6. EF Core DbContext (`Infrastructure/Persistence/ManagementDbContext.cs`)

```csharp
using Management.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Management.Api.Infrastructure.Persistence;

public class ManagementDbContext(DbContextOptions<ManagementDbContext> options) : DbContext(options)
{
    public DbSet<Advisor>           Advisors           => Set<Advisor>();
    public DbSet<Workshop>          Workshops          => Set<Workshop>();
    public DbSet<Brand>             Brands             => Set<Brand>();
    public DbSet<AdvisorBrand>      AdvisorBrands      => Set<AdvisorBrand>();
    public DbSet<ServiceType>       ServiceTypes       => Set<ServiceType>();
    public DbSet<AdvisorServiceType> AdvisorServiceTypes => Set<AdvisorServiceType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

---

## 7. Vertical slices

---

### 7.1 GET /api/advisors — paginated list with filters

#### `GetAdvisorsRequestDto.cs`

```csharp
namespace Management.Api.Features.Advisors.GetAdvisors;

public record GetAdvisorsRequestDto(
    string? NameFilter     = null,
    string? WorkshopFilter = null,
    string? BrandFilter    = null,
    string? ServiceFilter  = null,
    int     PageNumber     = 1,
    int     PageSize       = 10
);
```

#### `GetAdvisorsResponseDto.cs`

```csharp
namespace Management.Api.Features.Advisors.GetAdvisors;

public record GetAdvisorsResponseDto(
    IEnumerable<AdvisorRowDto> Items,
    int                        TotalCount,
    int                        PageNumber,
    int                        PageSize
);

public record AdvisorRowDto(
    Guid    AdvisorId,
    string  FullName,
    string? Phone,
    bool    Status,
    Guid?   WorkshopId,
    string? WorkshopName,
    string? Brands,       // comma-separated from SP
    string? Services      // comma-separated from SP
);
```

#### `GetAdvisorsQuery.cs` — executes stored procedure

```csharp
using MediatR;
using Management.Api.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Management.Api.Features.Advisors.GetAdvisors;

// ── Query ─────────────────────────────────────────────────────────────────
public record GetAdvisorsQuery(GetAdvisorsRequestDto Filters) : IRequest<GetAdvisorsResponseDto>;

// ── Handler ───────────────────────────────────────────────────────────────
public class GetAdvisorsQueryHandler(ManagementDbContext db)
    : IRequestHandler<GetAdvisorsQuery, GetAdvisorsResponseDto>
{
    public async Task<GetAdvisorsResponseDto> Handle(
        GetAdvisorsQuery  request,
        CancellationToken ct)
    {
        var f = request.Filters;

        var totalParam = new SqlParameter("@TotalCount", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        var rows = await db.Database
            .SqlQueryRaw<AdvisorRowDto>(
                "EXEC sp_GetAdvisors @NameFilter, @WorkshopFilter, @BrandFilter, @ServiceFilter, @PageNumber, @PageSize, @TotalCount OUTPUT",
                new SqlParameter("@NameFilter",     (object?)f.NameFilter     ?? DBNull.Value),
                new SqlParameter("@WorkshopFilter", (object?)f.WorkshopFilter ?? DBNull.Value),
                new SqlParameter("@BrandFilter",    (object?)f.BrandFilter    ?? DBNull.Value),
                new SqlParameter("@ServiceFilter",  (object?)f.ServiceFilter  ?? DBNull.Value),
                new SqlParameter("@PageNumber",     f.PageNumber),
                new SqlParameter("@PageSize",       f.PageSize),
                totalParam)
            .ToListAsync(ct);

        var total = totalParam.Value is int t ? t : 0;

        return new GetAdvisorsResponseDto(rows, total, f.PageNumber, f.PageSize);
    }
}
```

#### `GetAdvisorsEndpoint.cs`

```csharp
using MediatR;

namespace Management.Api.Features.Advisors.GetAdvisors;

public static class GetAdvisorsEndpoint
{
    public static void MapGetAdvisors(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/advisors", async (
            [AsParameters] GetAdvisorsRequestDto filters,
            IMediator         mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAdvisorsQuery(filters), ct);
            return Results.Ok(result);
        })
        .WithName("GetAdvisors")
        .WithTags("Advisors")
        .WithSummary("Get paginated and filtered list of advisors")
        .Produces<GetAdvisorsResponseDto>()
        .RequireAuthorization();
    }
}
```

---

### 7.2 GET /api/advisors/{id} — single advisor detail

#### `AdvisorDetailResponseDto.cs`

```csharp
namespace Management.Api.Features.Advisors.GetAdvisorById;

public record AdvisorDetailResponseDto(
    Guid                  AdvisorId,
    string                FirstName,
    string                LastName,
    string?               Phone,
    bool                  Status,
    Guid?                 WorkshopId,
    string?               WorkshopName,
    IEnumerable<BrandDto>       Brands,
    IEnumerable<ServiceTypeDto> Services
);

public record BrandDto(Guid BrandId, string BrandName);
public record ServiceTypeDto(Guid ServiceTypeId, string ServiceName);
```

#### `GetAdvisorByIdQuery.cs`

```csharp
using MediatR;
using Management.Api.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Management.Api.Features.Advisors.GetAdvisorById;

public record GetAdvisorByIdQuery(Guid AdvisorId) : IRequest<AdvisorDetailResponseDto>;

public class GetAdvisorByIdQueryHandler(ManagementDbContext db)
    : IRequestHandler<GetAdvisorByIdQuery, AdvisorDetailResponseDto>
{
    public async Task<AdvisorDetailResponseDto> Handle(
        GetAdvisorByIdQuery request,
        CancellationToken   ct)
    {
        // EF Core executes SP and maps the first result set
        var advisor = await db.Advisors
            .Include(a => a.Workshop)
            .Include(a => a.AdvisorBrands).ThenInclude(ab => ab.Brand)
            .Include(a => a.AdvisorServiceTypes).ThenInclude(ast => ast.ServiceType)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AdvisorId == request.AdvisorId, ct)
            ?? throw new KeyNotFoundException($"Advisor {request.AdvisorId} not found.");

        return new AdvisorDetailResponseDto(
            AdvisorId:   advisor.AdvisorId,
            FirstName:   advisor.FirstName,
            LastName:    advisor.LastName,
            Phone:       advisor.Phone,
            Status:      advisor.Status,
            WorkshopId:  advisor.WorkshopId,
            WorkshopName: advisor.Workshop?.Name,
            Brands:       advisor.AdvisorBrands.Select(ab  => new BrandDto(ab.BrandId, ab.Brand.Name)),
            Services:     advisor.AdvisorServiceTypes.Select(ast => new ServiceTypeDto(ast.ServiceTypeId, ast.ServiceType.Name))
        );
    }
}
```

#### `GetAdvisorByIdEndpoint.cs`

```csharp
using MediatR;

namespace Management.Api.Features.Advisors.GetAdvisorById;

public static class GetAdvisorByIdEndpoint
{
    public static void MapGetAdvisorById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/advisors/{id:guid}", async (
            Guid              id,
            IMediator         mediator,
            CancellationToken ct) =>
        {
            try
            {
                var result = await mediator.Send(new GetAdvisorByIdQuery(id), ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { detail = ex.Message });
            }
        })
        .WithName("GetAdvisorById")
        .WithTags("Advisors")
        .Produces<AdvisorDetailResponseDto>()
        .Produces(404)
        .RequireAuthorization();
    }
}
```

---

### 7.3 POST /api/advisors — create advisor (EF Core)

#### `CreateAdvisorRequestDto.cs`

```csharp
namespace Management.Api.Features.Advisors.CreateAdvisor;

public record CreateAdvisorRequestDto(
    string        FirstName,
    string        LastName,
    string?       Phone,
    Guid?         WorkshopId,
    List<Guid>    BrandIds,
    List<Guid>    ServiceTypeIds,
    bool          Status = true
);
```

#### `CreateAdvisorResponseDto.cs`

```csharp
namespace Management.Api.Features.Advisors.CreateAdvisor;

public record CreateAdvisorResponseDto(Guid AdvisorId, string FullName);
```

#### `CreateAdvisorValidator.cs`

```csharp
using FluentValidation;

namespace Management.Api.Features.Advisors.CreateAdvisor;

public class CreateAdvisorValidator : AbstractValidator<CreateAdvisorRequestDto>
{
    public CreateAdvisorValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(20).When(x => x.Phone is not null);

        RuleFor(x => x.BrandIds)
            .NotEmpty().WithMessage("At least one brand must be selected.");

        RuleFor(x => x.ServiceTypeIds)
            .NotEmpty().WithMessage("At least one service type must be selected.");
    }
}
```

#### `CreateAdvisorCommand.cs`

```csharp
using MediatR;
using Management.Api.Domain.Entities;
using Management.Api.Infrastructure.Persistence;

namespace Management.Api.Features.Advisors.CreateAdvisor;

// ── Command ───────────────────────────────────────────────────────────────
public record CreateAdvisorCommand(CreateAdvisorRequestDto Dto) : IRequest<CreateAdvisorResponseDto>;

// ── Handler (EF Core — standard CRUD) ────────────────────────────────────
public class CreateAdvisorCommandHandler(ManagementDbContext db)
    : IRequestHandler<CreateAdvisorCommand, CreateAdvisorResponseDto>
{
    public async Task<CreateAdvisorResponseDto> Handle(
        CreateAdvisorCommand command,
        CancellationToken    ct)
    {
        var dto = command.Dto;

        var advisor = new Advisor
        {
            AdvisorId  = Guid.NewGuid(),
            FirstName  = dto.FirstName.Trim(),
            LastName   = dto.LastName.Trim(),
            Phone      = dto.Phone?.Trim(),
            WorkshopId = dto.WorkshopId,
            Status     = dto.Status,
            CreatedAt  = DateTime.UtcNow,
            UpdatedAt  = DateTime.UtcNow
        };

        // Junction records — brands
        foreach (var brandId in dto.BrandIds.Distinct())
            advisor.AdvisorBrands.Add(new AdvisorBrand
            {
                AdvisorBrandId = Guid.NewGuid(),
                AdvisorId      = advisor.AdvisorId,
                BrandId        = brandId
            });

        // Junction records — service types
        foreach (var serviceTypeId in dto.ServiceTypeIds.Distinct())
            advisor.AdvisorServiceTypes.Add(new AdvisorServiceType
            {
                AdvisorServiceTypeId = Guid.NewGuid(),
                AdvisorId            = advisor.AdvisorId,
                ServiceTypeId        = serviceTypeId
            });

        db.Advisors.Add(advisor);
        await db.SaveChangesAsync(ct);

        return new CreateAdvisorResponseDto(
            advisor.AdvisorId,
            $"{advisor.FirstName} {advisor.LastName}");
    }
}
```

#### `CreateAdvisorEndpoint.cs`

```csharp
using FluentValidation;
using MediatR;

namespace Management.Api.Features.Advisors.CreateAdvisor;

public static class CreateAdvisorEndpoint
{
    public static void MapCreateAdvisor(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/advisors", async (
            CreateAdvisorRequestDto          dto,
            IValidator<CreateAdvisorRequestDto> validator,
            IMediator                        mediator,
            CancellationToken                ct) =>
        {
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await mediator.Send(new CreateAdvisorCommand(dto), ct);
            return Results.Created($"/api/advisors/{result.AdvisorId}", result);
        })
        .WithName("CreateAdvisor")
        .WithTags("Advisors")
        .Produces<CreateAdvisorResponseDto>(201)
        .ProducesValidationProblem()
        .RequireAuthorization();
    }
}
```

---

### 7.4 PUT /api/advisors/{id} — update advisor (EF Core)

#### `UpdateAdvisorRequestDto.cs`

```csharp
namespace Management.Api.Features.Advisors.UpdateAdvisor;

public record UpdateAdvisorRequestDto(
    string     FirstName,
    string     LastName,
    string?    Phone,
    Guid?      WorkshopId,
    List<Guid> BrandIds,
    List<Guid> ServiceTypeIds
);
```

#### `UpdateAdvisorValidator.cs`

```csharp
using FluentValidation;

namespace Management.Api.Features.Advisors.UpdateAdvisor;

public class UpdateAdvisorValidator : AbstractValidator<UpdateAdvisorRequestDto>
{
    public UpdateAdvisorValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BrandIds).NotEmpty().WithMessage("At least one brand is required.");
        RuleFor(x => x.ServiceTypeIds).NotEmpty().WithMessage("At least one service type is required.");
    }
}
```

#### `UpdateAdvisorCommand.cs`

```csharp
using MediatR;
using Management.Api.Domain.Entities;
using Management.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Management.Api.Features.Advisors.UpdateAdvisor;

public record UpdateAdvisorCommand(Guid AdvisorId, UpdateAdvisorRequestDto Dto) : IRequest<Unit>;

public class UpdateAdvisorCommandHandler(ManagementDbContext db)
    : IRequestHandler<UpdateAdvisorCommand, Unit>
{
    public async Task<Unit> Handle(UpdateAdvisorCommand command, CancellationToken ct)
    {
        var advisor = await db.Advisors
            .Include(a => a.AdvisorBrands)
            .Include(a => a.AdvisorServiceTypes)
            .FirstOrDefaultAsync(a => a.AdvisorId == command.AdvisorId, ct)
            ?? throw new KeyNotFoundException($"Advisor {command.AdvisorId} not found.");

        var dto = command.Dto;

        // Update scalar fields
        advisor.FirstName  = dto.FirstName.Trim();
        advisor.LastName   = dto.LastName.Trim();
        advisor.Phone      = dto.Phone?.Trim();
        advisor.WorkshopId = dto.WorkshopId;
        advisor.UpdatedAt  = DateTime.UtcNow;

        // Replace brands — remove old, add new
        db.RemoveRange(advisor.AdvisorBrands);
        foreach (var brandId in dto.BrandIds.Distinct())
            advisor.AdvisorBrands.Add(new AdvisorBrand
            {
                AdvisorBrandId = Guid.NewGuid(),
                AdvisorId      = advisor.AdvisorId,
                BrandId        = brandId
            });

        // Replace service types
        db.RemoveRange(advisor.AdvisorServiceTypes);
        foreach (var serviceTypeId in dto.ServiceTypeIds.Distinct())
            advisor.AdvisorServiceTypes.Add(new AdvisorServiceType
            {
                AdvisorServiceTypeId = Guid.NewGuid(),
                AdvisorId            = advisor.AdvisorId,
                ServiceTypeId        = serviceTypeId
            });

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
```

#### `UpdateAdvisorEndpoint.cs`

```csharp
using FluentValidation;
using MediatR;

namespace Management.Api.Features.Advisors.UpdateAdvisor;

public static class UpdateAdvisorEndpoint
{
    public static void MapUpdateAdvisor(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/advisors/{id:guid}", async (
            Guid                             id,
            UpdateAdvisorRequestDto          dto,
            IValidator<UpdateAdvisorRequestDto> validator,
            IMediator                        mediator,
            CancellationToken                ct) =>
        {
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                await mediator.Send(new UpdateAdvisorCommand(id, dto), ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { detail = ex.Message });
            }
        })
        .WithName("UpdateAdvisor")
        .WithTags("Advisors")
        .Produces(204)
        .Produces(404)
        .ProducesValidationProblem()
        .RequireAuthorization();
    }
}
```

---

### 7.5 PATCH /api/advisors/{id}/status — toggle status (EF Core)

#### `ToggleAdvisorStatusRequestDto.cs`

```csharp
namespace Management.Api.Features.Advisors.ToggleAdvisorStatus;

public record ToggleAdvisorStatusRequestDto(bool Status);
```

#### `ToggleAdvisorStatusCommand.cs`

```csharp
using MediatR;
using Management.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Management.Api.Features.Advisors.ToggleAdvisorStatus;

public record ToggleAdvisorStatusCommand(Guid AdvisorId, bool Status) : IRequest<Unit>;

public class ToggleAdvisorStatusCommandHandler(ManagementDbContext db)
    : IRequestHandler<ToggleAdvisorStatusCommand, Unit>
{
    public async Task<Unit> Handle(ToggleAdvisorStatusCommand command, CancellationToken ct)
    {
        var advisor = await db.Advisors
            .FirstOrDefaultAsync(a => a.AdvisorId == command.AdvisorId, ct)
            ?? throw new KeyNotFoundException($"Advisor {command.AdvisorId} not found.");

        advisor.Status    = command.Status;
        advisor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
```

#### `ToggleAdvisorStatusEndpoint.cs`

```csharp
using MediatR;

namespace Management.Api.Features.Advisors.ToggleAdvisorStatus;

public static class ToggleAdvisorStatusEndpoint
{
    public static void MapToggleAdvisorStatus(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/advisors/{id:guid}/status", async (
            Guid                            id,
            ToggleAdvisorStatusRequestDto   dto,
            IMediator                       mediator,
            CancellationToken               ct) =>
        {
            try
            {
                await mediator.Send(new ToggleAdvisorStatusCommand(id, dto.Status), ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { detail = ex.Message });
            }
        })
        .WithName("ToggleAdvisorStatus")
        .WithTags("Advisors")
        .Produces(204)
        .Produces(404)
        .RequireAuthorization();
    }
}
```

---

### 7.6 DELETE /api/advisors/{id} — delete advisor (stored procedure — transactional)

#### `DeleteAdvisorCommand.cs`

```csharp
using MediatR;
using Management.Api.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace Management.Api.Features.Advisors.DeleteAdvisor;

public record DeleteAdvisorCommand(Guid AdvisorId) : IRequest<Unit>;

public class DeleteAdvisorCommandHandler(ManagementDbContext db)
    : IRequestHandler<DeleteAdvisorCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAdvisorCommand command, CancellationToken ct)
    {
        // SP handles transactional deletion of junction records + main row
        await db.Database.ExecuteSqlRawAsync(
            "EXEC sp_DeleteAdvisor @AdvisorId",
            new SqlParameter("@AdvisorId", command.AdvisorId),
            ct);

        return Unit.Value;
    }
}
```

#### `DeleteAdvisorEndpoint.cs`

```csharp
using MediatR;

namespace Management.Api.Features.Advisors.DeleteAdvisor;

public static class DeleteAdvisorEndpoint
{
    public static void MapDeleteAdvisor(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/advisors/{id:guid}", async (
            Guid              id,
            IMediator         mediator,
            CancellationToken ct) =>
        {
            try
            {
                await mediator.Send(new DeleteAdvisorCommand(id), ct);
                return Results.NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return Results.NotFound(new { detail = ex.Message });
            }
        })
        .WithName("DeleteAdvisor")
        .WithTags("Advisors")
        .Produces(204)
        .Produces(404)
        .RequireAuthorization();
    }
}
```

---

## 8. `Program.cs` — Management API

```csharp
using FluentValidation;
using Management.Api.BuildingBlocks.Middleware;
using Management.Api.Features.Advisors.GetAdvisors;
using Management.Api.Features.Advisors.GetAdvisorById;
using Management.Api.Features.Advisors.CreateAdvisor;
using Management.Api.Features.Advisors.UpdateAdvisor;
using Management.Api.Features.Advisors.ToggleAdvisorStatus;
using Management.Api.Features.Advisors.DeleteAdvisor;
using Management.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── MediatR ───────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ── FluentValidation ──────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<CreateAdvisorValidator>();

// ── JWT validation (tokens issued by Login.Api) ───────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret not configured.");

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

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(",")
                ?? ["http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

// ── Register all Advisor endpoints ────────────────────────────────────────
app.MapGetAdvisors();
app.MapGetAdvisorById();
app.MapCreateAdvisor();
app.MapUpdateAdvisor();
app.MapToggleAdvisorStatus();
app.MapDeleteAdvisor();

app.Run();
```

---

## 9. API contract summary

| Method | Path | Body / Query | Auth | Handler |
|--------|------|-------------|------|---------|
| `GET` | `/api/advisors` | Query: `nameFilter`, `workshopFilter`, `brandFilter`, `serviceFilter`, `pageNumber`, `pageSize` | JWT | Stored Procedure |
| `GET` | `/api/advisors/{id}` | — | JWT | EF Core |
| `POST` | `/api/advisors` | `CreateAdvisorRequestDto` | JWT | EF Core |
| `PUT` | `/api/advisors/{id}` | `UpdateAdvisorRequestDto` | JWT | EF Core |
| `PATCH` | `/api/advisors/{id}/status` | `{ "status": true }` | JWT | EF Core |
| `DELETE` | `/api/advisors/{id}` | — | JWT | Stored Procedure |

### Example — `POST /api/advisors` request body

```json
{
  "firstName":      "Javier",
  "lastName":       "Torres",
  "phone":          "562302145",
  "workshopId":     "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "brandIds":       ["b1a2...", "c3d4..."],
  "serviceTypeIds": ["e5f6...", "a7b8..."],
  "status":         true
}
```

### Example — `GET /api/advisors` response

```json
{
  "items": [
    {
      "advisorId":    "3fa85f64-...",
      "fullName":     "Javier Torres",
      "phone":        "562302145",
      "status":       true,
      "workshopId":   "...",
      "workshopName": "STK Power",
      "brands":       "BMW, Subaru",
      "services":     "Mechanical repairs, Preventive maintenance"
    }
  ],
  "totalCount": 13,
  "pageNumber":  1,
  "pageSize":   10
}
```

---

## 10. Decision log — EF Core vs Stored Procedure per slice

| Slice | Strategy | Reason |
|-------|----------|--------|
| `GetAdvisors` | **Stored Procedure** | Requires `STRING_AGG` across junction tables, dynamic filters, server-side pagination and `OUTPUT` param for total count |
| `GetAdvisorById` | **EF Core** (`Include`) | Simple eager loading across 3 related sets — no aggregation needed |
| `CreateAdvisor` | **EF Core** | Standard insert across 3 tables — EF handles the graph cleanly |
| `UpdateAdvisor` | **EF Core** | Replace junction records + update scalar fields — transactional via `SaveChangesAsync` |
| `ToggleAdvisorStatus` | **EF Core** | Single-field update — one round-trip |
| `DeleteAdvisor` | **Stored Procedure** | Explicit transaction required across multiple junction tables — `XACT_ABORT ON` guarantees atomicity |
