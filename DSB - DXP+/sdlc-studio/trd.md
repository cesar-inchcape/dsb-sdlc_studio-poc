# Technical Requirements Document

**Project:** Digital Service Booking (DSB) - DXP+  
**Version:** 1.0.0  
**Status:** Draft  
**Last Updated:** 2026-05-28  
**PRD Reference:** [PRD](prd.md)

---

## 1. Executive Summary

### Purpose
Define the technical architecture, technology stack, and implementation patterns for the Digital Service Booking (DSB) platform, serving third-party automotive dealers (Americar and Autum) in Chile with multi-brand workshop management and service booking capabilities.

### Scope
This TRD covers:
- Backend architecture (3 microservices: Login.Api, Management.Api, Booking.Api - Phase 2)
- Frontend architecture (React + TypeScript + Atomic Design)
- Database design (.NET Entity Framework Core + SQL Server + Stored Procedures)
- API contracts (RESTful APIs with JWT authentication)
- Security implementation (BCrypt password hashing, HTTPS, CORS)
- Development and testing standards

**Out of Scope (Phase 1):**
- Booking.Api microservice (deferred to Phase 2)
- Reporting & Analytics features
- DMS Integration
- Email/SMS notifications
- Payment gateway integration

### Key Decisions
- **Vertical Slice Architecture** for backend organization (features over layers)
- **Hybrid data access**: Entity Framework Core for CRUD + SQL Server Stored Procedures for complex business logic
- **Microservices pattern** with 3 independent APIs (Phase 1: 2 APIs only)
- **Atomic Design pattern** for frontend component structure
- **JWT-based authentication** with role-based access control (RBAC)
- **Backend-first implementation** (APIs before Frontend)

---

## 2. Project Classification

**Project Type:** API Backend + Web Application (Full-Stack)

**Classification Rationale:** 
The DSB platform consists of:
1. **Backend APIs** providing RESTful endpoints for authentication, user management, workshop configuration, and advisor scheduling
2. **Web Application** (React SPA) consuming these APIs for dealer and workshop user interfaces

**Architecture Implications:**
- **Default Pattern:** Modular Monolith (for typical API backends)
- **Pattern Used:** Microservices
- **Deviation Rationale:** 
  - Business domains are well-defined and independent (Authentication, Management, Booking)
  - Different scalability requirements per domain (Booking will have high traffic)
  - Future DMS integration requires clear service boundaries
  - Team organization can align with service ownership

---

## 3. Architecture Overview

### System Context
The DSB platform sits between **dealer administrators, workshop staff, and end customers** on one side, and **SQL Server database, future DMS systems, and notification services** on the other side. It provides:
- Secure authentication and authorization
- Workshop and advisor master data management
- Service booking and availability calculation (Phase 2)
- Operational reporting and dashboards (Phase 2)

**External Systems (Future):**
- DMS (Dealer Management System) for customer/vehicle data sync
- Email/SMS provider for appointment reminders
- Payment gateway for online payments

### Architecture Pattern
**Microservices** with **Vertical Slice Architecture** within each service

**Rationale:** 
- Microservices enable independent deployment and scaling of login, management, and booking services
- Vertical Slice Architecture (within each service) organizes code by features rather than technical layers, improving maintainability and team autonomy
- This hybrid approach balances service boundaries with feature cohesion

### Component Overview

| Component | Responsibility | Technology |
|-----------|---------------|------------|
| **Login.Api** | Authentication, authorization, JWT token generation, RBAC | .NET 10, ASP.NET Core Web API |
| **Management.Api** | Workshops, Advisors, Users, Client Database (CRUD + scheduling config) | .NET 10, ASP.NET Core Web API |
| **Booking.Api** (Phase 2) | Service bookings, availability calculation, dashboards, reports | .NET 10, ASP.NET Core Web API |
| **Frontend (3pd_booking_fe)** | User interface for all roles (React SPA with Atomic Design) | React 18 + TypeScript + Vite |
| **SQL Server Database** | Relational data storage (users, roles, workshops, advisors, bookings) | SQL Server (with EF Core + Stored Procedures) |

---

## 4. Technology Stack

### Core Technologies

| Category | Technology | Version | Rationale |
|----------|-----------|---------|-----------|
| **Backend Language** | C# | .NET 10 | Enterprise-grade framework with strong typing, async/await support, EF Core integration, and excellent tooling for API development |
| **Backend Framework** | ASP.NET Core Web API | .NET 10 | Lightweight, high-performance REST API framework with built-in dependency injection, middleware pipeline, and OpenAPI support |
| **Database** | SQL Server | Latest | ACID compliance required for booking transactions, strong stored procedure support for complex queries, familiar to enterprise teams |
| **ORM** | Entity Framework Core | Latest (EF Core 10) | Type-safe data access, migrations support, parameterized queries (SQL injection prevention) |
| **Frontend Framework** | React | 18.3.1 | Component-based architecture aligns with Atomic Design, large ecosystem, TypeScript support |
| **Frontend Language** | TypeScript | 5.5.3 | Type safety reduces runtime errors, better IDE support, refactoring confidence |
| **Build Tool** | Vite | 5.3.4 | Fast HMR (Hot Module Replacement), modern ES modules, optimized production builds |

### Build & Development

| Tool | Purpose |
|------|---------|
| **MediatR** | CQRS pattern implementation (Command/Query handlers within Vertical Slices) |
| **FluentValidation** | Request DTO validation with declarative rules |
| **BCrypt.Net** or **Argon2** | Secure password hashing (no plaintext passwords) |
| **Swagger/OpenAPI** | Interactive API documentation and testing |
| **Serilog** | Structured logging with sinks (file, console, future: Application Insights) |
| **xUnit** | Backend unit testing framework |
| **Jest** or **Vitest** | Frontend unit testing framework |
| **Axios** | HTTP client for API calls (with interceptors for auth tokens) |
| **ESLint** | TypeScript/React code quality and style enforcement |

### Infrastructure Services

| Service | Provider | Purpose |
|---------|----------|---------|
| **Database Hosting** | [TBD - Localhost/Azure SQL] | SQL Server instance for development and production |
| **Container Orchestration** | Docker (dev), [TBD - K8s/Azure Container Apps] | Service isolation, consistent environments |
| **Cache (Future)** | Redis | Session state, rate limiting, hot data caching |
| **Message Bus (Future)** | RabbitMQ or Azure Service Bus | Async communication between services, event-driven patterns |

---

## 5. API Contracts

### API Style
**REST** (Resource-oriented URLs, HTTP verbs, JSON payloads)

### Authentication
**JWT (JSON Web Tokens)** with Bearer scheme

**Flow:**
1. User submits credentials to `POST /api/auth/login` (Login.Api)
2. Login.Api validates credentials (BCrypt hash comparison)
3. Returns JWT token with claims (user ID, roles, permissions)
4. Client includes token in `Authorization: Bearer <token>` header for all subsequent requests
5. APIs validate token signature and extract claims for authorization

**Token Claims:**
- `sub`: User ID (GUID)
- `email`: User email
- `roles`: Array of role names
- `permissions`: Array of permission codes
- `exp`: Expiration timestamp (60 minutes default)

### Endpoints Overview (Phase 1)

#### Login.Api

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/api/auth/login` | Authenticate user, return JWT | No |
| POST | `/api/auth/refresh` | Refresh expired JWT | Yes (refresh token) |
| POST | `/api/auth/logout` | Invalidate token (future: blacklist) | Yes |
| GET | `/api/auth/me` | Get current user profile | Yes |

#### Management.Api

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/api/users` | List users (with filters) | Yes (Admin) |
| POST | `/api/users` | Create user | Yes (Admin) |
| GET | `/api/users/{id}` | Get user details | Yes |
| PUT | `/api/users/{id}` | Update user | Yes (Admin) |
| DELETE | `/api/users/{id}` | Delete user | Yes (Admin) |
| GET | `/api/workshops` | List workshops | Yes |
| POST | `/api/workshops` | Create workshop | Yes (Admin) |
| GET | `/api/workshops/{id}` | Get workshop details | Yes |
| PUT | `/api/workshops/{id}` | Update workshop | Yes (Admin) |
| GET | `/api/advisors` | List advisors | Yes |
| POST | `/api/advisors` | Create advisor | Yes (Admin) |
| GET | `/api/advisors/{id}` | Get advisor details | Yes |
| PUT | `/api/advisors/{id}` | Update advisor | Yes (Admin) |

**Versioning Strategy:** URL versioning (`/api/v1/users`, `/api/v2/users`) for breaking changes

**Rate Limiting:** [TBD - 100 requests/minute per user]

### Request/Response Format

**Request (Example: Create User):**
```json
POST /api/users
Authorization: Bearer <jwt_token>
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isActive": true
}
```

**Response (Success):**
```json
HTTP/1.1 201 Created
Content-Type: application/json

{
  "id": "8c7b5e3d-9f12-4a6b-8d2e-1f3c5a7b9e4d",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "role": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Workshop User"
  },
  "isActive": true,
  "createdAt": "2026-05-28T14:30:00Z"
}
```

### Error Response Format
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": {
      "email": ["Email is required"],
      "firstName": ["First name must be at least 2 characters"]
    }
  }
}
```

**Standard Error Codes:**
- `UNAUTHORIZED` (401): Missing or invalid JWT token
- `FORBIDDEN` (403): Valid token but insufficient permissions
- `VALIDATION_ERROR` (422): Request DTO validation failed
- `NOT_FOUND` (404): Resource does not exist
- `CONFLICT` (409): Duplicate resource (e.g., email already exists)
- `INTERNAL_ERROR` (500): Unhandled server exception

---

## 6. Data Architecture

### Data Access Strategy

**Hybrid Approach:**
1. **Entity Framework Core** for basic CRUD operations:
   - Simple create, read, update, delete on entities
   - Type-safe LINQ queries
   - Automatic change tracking
   - Database migrations

2. **SQL Server Stored Procedures** (executed via EF Core) for:
   - Complex business logic (availability calculations, multi-table aggregations)
   - Custom queries with dynamic filters
   - Reports and dashboards
   - Operations requiring transactional control
   - Performance-critical queries

**Why this hybrid approach?**
- EF Core simplifies standard CRUD and migrations
- Stored Procedures optimize complex queries and leverage SQL Server's query planner
- Parameterization (both EF Core and SPs) prevents SQL injection

### Data Models (Phase 1)

#### Users
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | uniqueidentifier (GUID) | PK | User unique identifier |
| Email | nvarchar(256) | NOT NULL, UNIQUE | User email (login username) |
| PasswordHash | nvarchar(max) | NOT NULL | BCrypt or Argon2 hashed password |
| FirstName | nvarchar(100) | NOT NULL | User first name |
| LastName | nvarchar(100) | NOT NULL | User last name |
| IsActive | bit | NOT NULL, DEFAULT 1 | Account status (soft delete) |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |
| UpdatedAt | datetime2 | NULL | Last modification timestamp |

#### Roles
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | uniqueidentifier (GUID) | PK | Role unique identifier |
| Name | nvarchar(100) | NOT NULL, UNIQUE | Role name (e.g., "Super Administrator") |
| Description | nvarchar(500) | NULL | Role description |

#### Permissions
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | uniqueidentifier (GUID) | PK | Permission unique identifier |
| Code | nvarchar(100) | NOT NULL, UNIQUE | Permission code (e.g., "users.create") |
| Description | nvarchar(500) | NULL | Permission description |

#### UserRoles (Many-to-Many)
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| UserId | uniqueidentifier (GUID) | PK, FK → Users | User ID |
| RoleId | uniqueidentifier (GUID) | PK, FK → Roles | Role ID |

#### RolePermissions (Many-to-Many)
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| RoleId | uniqueidentifier (GUID) | PK, FK → Roles | Role ID |
| PermissionId | uniqueidentifier (GUID) | PK, FK → Permissions | Permission ID |

#### Workshops
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | uniqueidentifier (GUID) | PK | Workshop unique identifier |
| Name | nvarchar(200) | NOT NULL | Workshop name |
| Address | nvarchar(500) | NULL | Physical address |
| CityId | uniqueidentifier (GUID) | FK → Cities | City reference |
| IsActive | bit | NOT NULL, DEFAULT 1 | Operational status |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |

#### Advisors
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | uniqueidentifier (GUID) | PK | Advisor unique identifier |
| FirstName | nvarchar(100) | NOT NULL | Advisor first name |
| LastName | nvarchar(100) | NOT NULL | Advisor last name |
| WorkshopId | uniqueidentifier (GUID) | FK → Workshops | Assigned workshop |
| IsActive | bit | NOT NULL, DEFAULT 1 | Employment status |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |

#### WorkshopSchedules
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | uniqueidentifier (GUID) | PK | Schedule unique identifier |
| WorkshopId | uniqueidentifier (GUID) | FK → Workshops | Workshop reference |
| DayOfWeek | int | NOT NULL (0=Sunday, 6=Saturday) | Day of week |
| OpenTime | time | NOT NULL | Opening time |
| CloseTime | time | NOT NULL | Closing time |
| IsActive | bit | NOT NULL, DEFAULT 1 | Schedule status |

#### AdvisorSchedules
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | uniqueidentifier (GUID) | PK | Schedule unique identifier |
| AdvisorId | uniqueidentifier (GUID) | FK → Advisors | Advisor reference |
| DayOfWeek | int | NOT NULL (0=Sunday, 6=Saturday) | Day of week |
| StartTime | time | NOT NULL | Work start time |
| EndTime | time | NOT NULL | Work end time |
| IsActive | bit | NOT NULL, DEFAULT 1 | Schedule status |

### Storage Strategy

| Data Type | Storage | Rationale |
|-----------|---------|-----------|
| Transactional Data | SQL Server (on-disk tables) | ACID compliance for users, roles, workshops, advisors |
| User Passwords | BCrypt/Argon2 hashed strings (nvarchar) | Security requirement: no plaintext passwords |
| Audit Logs (Future) | SQL Server (append-only table) or separate log store | Compliance and debugging |
| Session State (Future) | Redis (in-memory cache) | Fast access for active user sessions, rate limiting |

### Migrations

**Approach:**
1. **EF Core Migrations** for table schema changes:
   - Generate migrations with `dotnet ef migrations add <name>`
   - Apply with `dotnet ef database update`
   - Track in `database/migrations/` folder (version control)

2. **SQL Scripts for Stored Procedures**:
   - Store in `database/stored_procedures/{api_name}/` folder
   - Use `CREATE OR ALTER PROCEDURE` syntax (idempotent)
   - Execute manually or via deployment scripts

**Migration Strategy:**
- Development: Auto-apply migrations on app startup (for rapid iteration)
- Production: Manual migration execution (via CI/CD pipeline) with rollback plans

---

## 7. Integration Patterns

### External Services (Future - Phase 2+)

| Service | Purpose | Protocol | Auth |
|---------|---------|----------|------|
| **DMS (Dealer Management System)** | Sync customer and vehicle data | REST API or SOAP | API Key or OAuth |
| **Email Provider** (SendGrid/AWS SES) | Appointment reminders, notifications | REST API | API Key |
| **SMS Provider** (Twilio) | SMS notifications | REST API | API Key |
| **Payment Gateway** (TBD) | Online payment processing | REST API | API Key + PCI compliance |

### Event Architecture (Future)
**Not implemented in Phase 1.**

Future consideration: Event-driven architecture for:
- Booking created → Trigger email/SMS notification
- Workshop schedule changed → Invalidate availability cache
- User role updated → Refresh permissions cache

**Potential Technologies:**
- RabbitMQ (self-hosted message broker)
- Azure Service Bus (managed cloud service)

---

## 8. Infrastructure

### Deployment Topology

**Phase 1 (Development):**
- Docker Compose with 4 containers:
  1. `login-api` (Login.Api)
  2. `management-api` (Management.Api)
  3. `sqlserver` (SQL Server database)
  4. `frontend` (React SPA - Nginx serving static files)

**Phase 2+ (Production - TBD):**
- Kubernetes cluster or Azure Container Apps
- Azure SQL Database (managed SQL Server)
- Azure Front Door (CDN for frontend assets)
- Application Insights (monitoring and logging)

### Environment Strategy

| Environment | Purpose | Characteristics |
|-------------|---------|-----------------|
| **Development** | Local development and testing | Docker Compose, SQL Server in container, hot reload enabled |
| **Staging** | Pre-production testing with production-like data | Azure-hosted, separate database, CI/CD auto-deploy |
| **Production** | Live system for dealers and customers | Azure-hosted, geo-redundant database, manual deploy approval |

**Environment Variables (per `.env.example`):**
- `DB_SERVER`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`: SQL Server connection details
- `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRY_MINUTES`: JWT configuration
- `REDIS_CONNECTION_STRING` (future): Redis cache connection
- `RABBITMQ_HOST` (future): Message broker connection

### Scaling Strategy

**Horizontal Scaling:**
- Login.Api and Management.Api: Stateless services, can scale to multiple instances behind load balancer
- Frontend: Static files served by CDN (Azure Front Door)

**Vertical Scaling:**
- SQL Server: Increase CPU/memory for production database (Azure SQL elastic pool)

**Bottleneck Mitigation:**
- Cache frequent queries (workshop schedules, advisor lists) in Redis
- Use database read replicas for reporting queries (future)

---

## 9. Security Considerations

### Threat Model

| Threat | Likelihood | Impact | Mitigation |
|--------|-----------|--------|------------|
| **SQL Injection** | Medium | Critical | Parameterized queries (EF Core + SP params), no dynamic SQL |
| **Credential Stuffing** | High | High | BCrypt/Argon2 password hashing, rate limiting on login endpoint |
| **Unauthorized Access** | High | Critical | JWT token validation, RBAC middleware, permission checks |
| **XSS (Cross-Site Scripting)** | Medium | Medium | React auto-escapes JSX, Content-Security-Policy headers |
| **CSRF (Cross-Site Request Forgery)** | Low | Medium | SameSite cookies (future), JWT in headers (not cookies) |
| **Man-in-the-Middle** | Medium | Critical | HTTPS only (redirect HTTP → HTTPS), TLS 1.2+ |
| **Data Breach** | Low | Critical | Encrypted database backups, access logs, least-privilege DB accounts |

### Security Controls

| Control | Implementation |
|---------|----------------|
| **Authentication** | JWT tokens with 60-minute expiry, BCrypt password hashing (salt rounds ≥ 12) |
| **Authorization** | RBAC middleware validates roles/permissions from JWT claims before endpoint execution |
| **Encryption at Rest** | SQL Server Transparent Data Encryption (TDE) enabled in production |
| **Encryption in Transit** | HTTPS enforced (redirect HTTP → HTTPS), TLS 1.2+ certificates |
| **Input Validation** | FluentValidation on all request DTOs, reject invalid payloads before business logic |
| **CORS** | Whitelist frontend origin (`https://booking.dealername.com`), reject other origins |
| **Secrets Management** | Environment variables (development), Azure Key Vault (production) |
| **Audit Logging** | Serilog logs all authentication attempts, authorization failures, data mutations |

### Password Security Requirements
- **Hashing Algorithm:** BCrypt (preferred) or Argon2
- **Salt Rounds (BCrypt):** Minimum 12 (balance security vs performance)
- **No Plaintext Storage:** Passwords never stored in plaintext or reversible encryption
- **Password Policy (Future):** Minimum 8 characters, mix of uppercase/lowercase/numbers/special chars

---

## 10. Performance Requirements

### Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| **API Response Time (p50)** | < 200ms | Application Insights / Prometheus |
| **API Response Time (p95)** | < 500ms | Application Insights / Prometheus |
| **API Response Time (p99)** | < 1000ms | Application Insights / Prometheus |
| **Frontend Initial Load (p50)** | < 2 seconds | Lighthouse / WebPageTest |
| **Frontend Interaction (p95)** | < 100ms | React DevTools Profiler |
| **Database Query Time (p95)** | < 100ms | SQL Server Query Store |
| **Throughput** | 100 requests/second per API | Load testing (Apache JMeter / k6) |
| **Availability** | 99.5% uptime | Azure Monitor |

**Performance Optimization Strategies:**
- Database indexes on foreign keys and frequently queried fields (WorkshopId, AdvisorId, DayOfWeek)
- Lazy loading for navigation properties (EF Core)
- Pagination for list endpoints (default: 20 items per page)
- Frontend code splitting (Vite lazy imports for routes)
- CDN caching for static assets (cache-control headers)

---

## 11. Architecture Decision Records

### ADR-001: Use Vertical Slice Architecture Instead of Layered Architecture

**Status:** Accepted

**Context:** 
Traditional layered architecture (Controllers → Services → Repositories) creates tight coupling across features and makes it difficult to modify a single feature without touching multiple layers. Teams often struggle with large Service and Repository classes that become dumping grounds for unrelated logic.

**Decision:** 
Organize backend code by **features** (Vertical Slices) instead of technical layers. Each feature folder contains its own controller, DTOs, command/query handlers, and data access logic.

Example:
```
Features/
  Users/
    CreateUser/
      CreateUserCommand.cs
      CreateUserHandler.cs
      CreateUserRequest.cs
      CreateUserResponse.cs
      CreateUserValidator.cs
    GetUser/
      GetUserQuery.cs
      GetUserHandler.cs
      ...
```

**Consequences:**
- **Positive:** Features are self-contained and independently modifiable. New developers can work on a feature without understanding the entire codebase. Reduces merge conflicts in large teams.
- **Negative:** Requires MediatR or similar pattern for command/query dispatching. May lead to code duplication across features (mitigate with shared BuildingBlocks).

---

### ADR-002: Use Entity Framework Core + Stored Procedures Hybrid

**Status:** Accepted

**Context:** 
Pure EF Core approach is clean but may generate suboptimal SQL for complex queries. Pure Stored Procedure approach loses type safety and requires manual DTO mapping. Need balance between developer productivity and performance.

**Decision:** 
Use **Entity Framework Core for basic CRUD** operations (create, read, update, delete) and **SQL Server Stored Procedures** (executed via EF Core) for complex queries, reports, and business logic requiring transactional control.

**Consequences:**
- **Positive:** Best of both worlds—type safety for CRUD, performance optimization for complex queries. EF Core migrations handle schema changes. Stored Procedures leverage SQL Server's query optimizer.
- **Negative:** Developers must know both EF Core and T-SQL. Migration scripts must handle both schema changes (EF Core) and stored procedure updates (SQL scripts).

---

### ADR-003: Backend-First Implementation (APIs Before Frontend)

**Status:** Accepted

**Context:** 
Frontend depends on API contracts to function. Starting frontend development before APIs are complete leads to mocking complexity and rework when real APIs differ from mocks.

**Decision:** 
Implement backend APIs (Login.Api, Management.Api) with full test coverage **before** starting frontend development. Frontend team uses Swagger documentation and API test instances.

**Consequences:**
- **Positive:** Frontend developers have stable, tested APIs to integrate against. Reduces integration issues. Backend team can focus on data integrity and business logic without frontend pressure.
- **Negative:** Frontend development starts later (longer overall timeline). Requires clear API contracts upfront (mitigate with OpenAPI specs).

---

### ADR-004: JWT Authentication with 60-Minute Expiry

**Status:** Accepted

**Context:** 
Need to balance security (short token expiry) with user experience (not forcing frequent logins). Session-based authentication requires server-side session storage (adds complexity for stateless APIs).

**Decision:** 
Use **JWT tokens** with **60-minute expiry** and refresh token mechanism (future). Tokens are stateless (validated via signature, not database lookup).

**Consequences:**
- **Positive:** Stateless authentication scales horizontally. No session storage required. Tokens can be validated by any API instance.
- **Negative:** Compromised token is valid until expiry (mitigate with short expiry + refresh tokens). Token revocation requires blacklist (future implementation).

---

### ADR-005: Atomic Design for Frontend Component Structure

**Status:** Accepted

**Context:** 
Flat component structure leads to inconsistent reuse and unclear component hierarchy. Need methodology for building scalable, reusable UI components.

**Decision:** 
Use **Atomic Design** pattern with 5 levels:
1. **Atoms** (Button, Input, Badge)
2. **Molecules** (FormField = Label + Input + Error)
3. **Organisms** (UsersTable, Sidebar, UserModal)
4. **Templates** (MainLayout)
5. **Pages** (UsersPage, WorkshopsPage)

**Consequences:**
- **Positive:** Clear component hierarchy. Promotes reusability (atoms and molecules used across multiple organisms). Easy to refactor styling at atomic level.
- **Negative:** Requires discipline to categorize components correctly. May feel over-engineered for small projects (acceptable trade-off for long-term maintainability).

---

## 12. Open Technical Questions

- [ ] **Q: Which specific version of SQL Server should be used?**
  **Context:** Backend blueprint specifies "SQL Server" but not version. SQL Server 2019, 2022, or Azure SQL? (Affects feature availability like temporal tables, JSON functions)
  
- [ ] **Q: Should we implement soft delete or hard delete for users/workshops/advisors?**
  **Context:** Current schema has `IsActive` flag (suggests soft delete). Need to confirm behavior for "deleted" records in reporting and auditing.

- [ ] **Q: What is the refresh token strategy for JWT?**
  **Context:** 60-minute expiry requires refresh mechanism. Should we use refresh tokens (stored in database), sliding expiry, or force re-login?

- [ ] **Q: Which Azure region should host production resources?**
  **Context:** Need to choose region closest to Chile for latency optimization (likely Azure Brazil South or US East).

- [ ] **Q: What is the backup and disaster recovery strategy?**
  **Context:** Production database needs backup frequency, retention period, and restore time objective (RTO) defined.

- [ ] **Q: Should we implement API rate limiting in Phase 1?**
  **Context:** Security best practice to prevent abuse, but adds complexity. Can defer to Phase 2 if not critical.

---

## 13. Implementation Constraints

### Must Have (Phase 1)
- **Backend must be implemented first** before frontend development starts
- **Unit tests and API tests required** for every feature (Definition of Done)
- **JWT authentication** with RBAC for all endpoints (except `/api/auth/login`)
- **BCrypt or Argon2 password hashing** (no plaintext passwords)
- **FluentValidation** for all request DTOs
- **Swagger/OpenAPI documentation** for all APIs
- **English naming conventions** for code, variables, and database objects (aligned with blueprints)
- **HTTPS enforced** in all non-local environments
- **Vertical Slice Architecture** strictly followed for backend features

### Won't Have (Phase 1)
- **Booking.Api microservice** (deferred to Phase 2)
- **Reporting and analytics features** (deferred to Phase 2)
- **DMS integration** (deferred to Phase 2)
- **Email/SMS notifications** (deferred to Phase 3)
- **Payment gateway integration** (deferred to Phase 3)
- **Redis caching** (future optimization)
- **Message queue / event bus** (future async processing)
- **Mobile app** (deferred to Phase 3)

---

## Changelog

| Date | Version | Changes |
|------|---------|---------|
| 2026-05-28 | 1.0.0 | Initial TRD generated from architecture blueprints (backend_blueprint.md, frontend_blueprint.md). Phase 1 scope defined (Login.Api + Management.Api only). |
