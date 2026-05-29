
# # AI Prompt Blueprint: Multi-API Booking System Backend (.NET 10 & SQL Server)

## Overview
This document serves as the absolute structural specification for building the backend and database architecture of the "BookingSystem" application. 

The system will be built using **.NET 10**, **ASP.NET Core Web API**, **Entity Framework Core**, and **SQL Server**. It must adhere strictly to **Vertical Slice Architecture**, **Clean Code principles**, and enterprise **Security Standards**.

Entity Framework Core will be used as the main data access technology. Basic CRUD operations will be handled through EF Core entities and DbContext. For custom queries, complex business logic, reports, validations, or transactional operations, the application will execute SQL Server Stored Procedures through Entity Framework Core.

---

## Architecture & Design Patterns

### 1. Vertical Slice Architecture
Instead of separating code by technical layers (Controllers, Services, Repositories), organize code by **Features** (e.g., `Features/Scheduling/CreateBooking`). Each slice must contain its own:
* **Controller / Endpoint definition** (Using Minimal APIs or Controllers)
* **Request/Response DTOs**
* **Domain Logic / Command / Query handlers** (MediatR is recommended)
* **Data Access logic** (Using Entity Framework Core for basic CRUD operations and executing SQL Server Stored Procedures through EF Core for custom queries or complex business logic)

### 2. Database & Data Access Strategy
* **Entity Framework Core:** EF Core will be used as the main data access technology for entity mapping, DbContext configuration, and basic CRUD operations.
* **Basic CRUD with EF Core:** Simple create, read, update, and delete operations must be implemented through EF Core entities and DbContext.
* **Stored Procedures for custom logic:** SQL Server Stored Procedures must be used for custom queries, complex filters, reports, validations, availability calculations, and operations that require transactional control.
* **Stored Procedures through EF Core:** Stored Procedures must be executed from the application through Entity Framework Core using parameterized calls.
* **SQL Server Mapping:** The source database schema must follow SQL Server conventions:
    * `uniqueidentifier` for GUID identifiers
    * `varchar(N)` / `nvarchar(N)` for text fields with defined length
    * `varchar(max)` / `nvarchar(max)` for large text fields
    * `datetime2` for date and time values
    * `bit` for boolean values
    * `int` for integer values
    * `time` for time-only values
    * `date` for date-only values
    * `decimal(18,2)` for monetary values

---

## Solution Folder Structure

The code must be partitioned into three independent API projects (Microservices/Modules) sharing a unified root structure. Scaffold the folders and placeholder files as follows:

```text
BookingSystem/
│
├── .env.example                       # Dummy local environment configurations
├── BookingSystem.sln                  # Main .NET Solution File
│
├── database/                          # SQL Server database management
│   ├── migrations/                    # EF Core migrations or SQL schema scripts
│   │   └── 001_initial_tables.sql     # Initial SQL Server DDL schema
│   └── stored_procedures/             # SQL Server Stored Procedure scripts
│       ├── login/
│       ├── booking/
│       └── management/
│
├── src/
│   ├── BuildingBlocks/                # Shared internal utilities (Extensions, Base Handlers)
│   │   ├── Security/
│   │   ├── Caching/                   # Future Redis/Cache abstractions
│   │   └── Messaging/                 # Future Message Bus/Queue abstractions
│   │
│   ├── Services/
│   │   ├── Login.Api/                 # 1. Login API
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   └── Features/
│   │   │       ├── Authentication/
│   │   │       └── Authorization/
│   │   │
│   │   ├── Booking.Api/               # 2. Booking API
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   └── Features/
│   │   │       ├── Scheduling/        # Modules: Bookings, AvailabilitySlots
│   │   │       ├── Dashboard/         # Real-time summary views
│   │   │       └── Reports/           # Module: BookingReports, Logs
│   │   │
│   │   └── Management.Api/            # 3. Management API
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── Features/
│   │           ├── Workshops/         # Workshops, WorkshopSchedules, WorkshopBlocks
│   │           ├── Advisors/          # Advisors, AdvisorSchedules, AdvisorBlocks
│   │           ├── Users/             # Base identity management profiles
│   │           └── ClientDatabase/    # Customers, Vehicles, Brands, Geography
│   │
└── tests/
    ├── Login.Api.UnitTests/
    ├── Login.Api.FeatureTests/        # Integration/E2E API Slice testing
    ├── Booking.Api.UnitTests/
    ├── Booking.Api.FeatureTests/
    ├── Management.Api.UnitTests/
    └── Management.Api.FeatureTests/

```

---

## API Module Allocations & Domain Boundaries

Map the attached `db_schema.txt` metadata directly across the 3 API projects as defined below:

### 1. Login API

Focuses exclusively on security, identity validation, tokens, and system accessibility.

* **Tables Managed:** `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions`.
* **Core Responsibilities:** Generate secure JWT tokens, match hashed credentials, and enforce RBAC (Role-Based Access Control) middlewares.

### 2. Booking API

Handles the high-throughput operational core of the system.

* **Tables Managed:** `Bookings`, `AvailabilitySlots`, `BookingReports`, `Logs`.
* **Internal Sub-Modules:**
* **Scheduling:** Booking mutations, reserving availability matrix.
* **Dashboard:** High-level active schedules processing.
* **Reports:** Generating summaries over `BookingReports` metrics.



### 3. Management API

The administrative master-data hub.

* **Tables Managed:** `Countries`, `Cities`, `DocumentTypes`, `Customers`, `VehicleData`, `ClientCompany`, `Retail`, `Workshops`, `Advisors`, `WorkshopSchedules`, `AdvisorSchedules`, `WorkshopBlocks`, `AdvisorBlocks`, `ServiceTypes`, `Brand`, `WorkshopBrand`.
* **Internal Sub-Modules:** `Workshops`, `Advisors`, `Users`, `Client Database`.

---

## Database Migration Strategy

Since the project uses Entity Framework Core and SQL Server, database versioning can be handled through a mixed strategy.

* **EF Core Migrations:** Use EF Core migrations for table schema changes related to entities managed through standard CRUD operations.
* **SQL Script Versioning:** Stored Procedures must be versioned as SQL scripts inside the `database/stored_procedures/` directory.
* **Stored Procedures Execution:** Stored Procedure scripts must use SQL Server syntax such as `CREATE OR ALTER PROCEDURE` so they can be updated iteratively without dropping core table states.

---

## Quality, Clean Code, & Security Standards

### Code Design Guidelines

* **Strict Use of DTOs:** Every feature endpoint must define explicit, immutable `RequestDto` and `ResponseDto` records. No domain database models or row-sets are allowed to escape past the API layer.
* **Validation:** Implement **FluentValidation** directly within each vertical slice to clean and evaluate incoming DTO payloads before executing business logic.
* **Async Everything:** Every database operation must use asynchronous paths. EF Core operations must use methods such as `ToListAsync`, `FirstOrDefaultAsync`, and `SaveChangesAsync`. Stored Procedure executions through EF Core must also be executed asynchronously when possible.

### Security Baselines

* **SQL Injection Prevention:** Absolutely no raw dynamic SQL string concatenations. Parameters must be passed safely through EF Core parameterization or explicitly typed SQL Server Stored Procedure parameters.
* **Password Protections:** Passwords inside the `Users` table must be transformed using strong hashing algorithms (**BCrypt** or **Argon2** via .NET cryptography layers). Never allow raw texts.
* **ASP.NET 8 Hardening:** Enforce HTTPS routing redirections, safe CORS control parameters, and prevent sensitive internal stack data from leaking via global exception-handling middlewares.

---

## Environment Configuration (`.env.example`)

Create a root level configuration blueprint using dummy parameters for local offline setups:

```env
# SQL Server Database Settings
DB_SERVER=localhost
DB_PORT=1433
DB_NAME=BookingSystemDev
DB_USER=dummy_db_user
DB_PASSWORD=dummy_db_password
DB_TRUST_SERVER_CERTIFICATE=true

# Connection String
CONNECTION_STRING=Server=localhost,1433;Database=BookingSystemDev;User Id=dummy_db_user;Password=dummy_db_password;TrustServerCertificate=True;

# Identity Security Keys
JWT_SECRET=this_is_a_dummy_secret_key_that_must_be_long_enough_32_bytes
JWT_ISSUER=BookingSystemAuthServer
JWT_AUDIENCE=BookingSystemApps
JWT_EXPIRY_MINUTES=60

# Future Infrastructures Variables
REDIS_CONNECTION_STRING=localhost:6379,password=dummy_redis_pass
RABBITMQ_HOST=localhost

```

---

## Execution Directives for the AI Agent

1. **Scaffold Structures:** Construct the complete .NET solution file along with project skeletons (`.csproj`) referencing .NET 10 and ASP.NET Core Web API target frameworks.
2. **Generate Folder Matrix:** Build out all `Features`, `BuildingBlocks`, and `Tests` directory structures exactly as illustrated in the structural visualizer graph.
3. **Produce Schema Shells:** Create the base `.sql` table schema blueprint inside `database/migrations/001_initial_tables.sql` matching SQL Server syntax.
4. **Inject Boilerplates:** Provide global application setup files (`Program.cs`, `appsettings.json`, and `.env.example`) containing basic configurations, missing completely functional coding details until specific vertical slice iterations begin.
5. **Data Access Guidance:** Use Entity Framework Core for basic CRUD operations. Use SQL Server Stored Procedures, executed through EF Core, for custom queries, complex business logic, reports, validations, and transactional operations.

```

```