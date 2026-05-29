# Test Strategy Document

> **Project:** Digital Service Booking (DSB) - DXP+
> **Version:** 1.0.0
> **Last Updated:** 2026-05-29
> **Owner:** TBD

## Overview

This Test Strategy Document defines the comprehensive testing approach for the DSB platform, covering Login.Api and Management.Api (Phase 1). The strategy emphasizes backend-first implementation with strict quality gates before Frontend integration.

## Test Objectives

- Ensure all authentication and authorization mechanisms are secure and functional
- Validate CRUD operations for users, workshops, and advisors meet acceptance criteria
- Achieve 90% code coverage for backend unit tests
- Verify API contracts and error handling for all endpoints
- Establish CI/CD quality gates to prevent regressions

## Scope

### In Scope
- **Login.Api:** JWT authentication, token refresh, role validation
- **Management.Api:** User, Workshop, Advisor CRUD and configuration endpoints
- Backend unit tests (.NET xUnit)
- Backend integration/API tests (HTTP client-based)
- Contract testing for API schemas
- Security testing (auth bypass, injection, RBAC enforcement)

### Out of Scope (Phase 1)
- Frontend E2E tests (deferred until Frontend implementation)
- Booking.Api tests (Phase 2)
- Performance/load testing (Phase 2)
- DMS integration testing (Phase 2+)

---

## Test Levels

### Coverage Targets

| Level | Target | Rationale |
|-------|--------|-----------|
| Unit | 90% | Core business logic and vertical slices |
| Integration | 90% | API endpoints and database interactions |
| E2E | Deferred | Awaiting Frontend implementation |

> **Why 90%?** Provides confidence in core functionality while allowing pragmatic exclusions (DTOs, migrations, trivial properties).

### Unit Testing

| Attribute | Value |
|-----------|-------|
| Coverage Target | 90% |
| Framework | xUnit (.NET 10) |
| Execution | `dotnet test` |
| Mocking | Moq (DbContext mocking) |
| Scope | Command/Query handlers, validators, business logic |

**Test Organization:**
```
tests/
  Login.Api.Tests/
    Features/
      Auth/
        LoginCommandHandlerTests.cs
        TokenValidationTests.cs
  Management.Api.Tests/
    Features/
      Users/
        CreateUserHandlerTests.cs
        UpdateUserHandlerTests.cs
      Workshops/
        CreateWorkshopHandlerTests.cs
      Advisors/
        CreateAdvisorHandlerTests.cs
```

### Integration Testing

| Attribute | Value |
|-----------|-------|
| Scope | HTTP endpoints, database round-trips, role enforcement |
| Framework | xUnit with WebApplicationFactory |
| Execution | `dotnet test --filter Category=Integration` |
| Database | SQL Server in Docker (test container) or in-memory |

**Coverage:**
- All API endpoints return correct status codes
- Request/response payloads match OpenAPI schema
- Authorization middleware enforces role restrictions
- Database constraints are validated

### API Contract Testing

> **Critical:** Contract tests validate API schemas, error codes, and payload structures to catch breaking changes early.

**Approach:**
- Define expected request/response shapes in test fixtures
- Validate HTTP status codes (200, 201, 400, 401, 403, 404, 422, 500)
- Assert error response structure matches documented format
- Verify RBAC enforcement (Super Admin, Distributor Admin, Workshop User)

### Security Testing

| Attribute | Value |
|-----------|-------|
| Scope | JWT validation, SQL injection prevention, RBAC enforcement |
| Tools | Manual security tests + OWASP dependency checks |

**Test Cases:**
- Invalid/expired JWT tokens return 401 Unauthorized
- Out-of-scope access attempts return 403 Forbidden
- SQL injection via parameterized queries (EF Core + Stored Procedures)
- Password hashing validation (BCrypt/Argon2)
- CORS policy enforcement

---

## Test Environments

| Environment | Purpose | URL | Data |
|-------------|---------|-----|------|
| Local | Development | localhost:5001 (Login), localhost:5002 (Management) | Seed data fixtures |
| Docker | Integration tests | Containerized SQL Server | Ephemeral test database |
| Staging | Pre-production | TBD | Sanitized production-like data |

## Test Data Strategy

### Approach
- **Unit tests:** In-memory mocks with minimal fixtures
- **Integration tests:** Seed scripts with known test data (users, roles, workshops, advisors)
- **API tests:** HTTP requests with JSON payloads from fixture files

### Sensitive Data
- No production data in test environments
- Passwords hashed even in test fixtures (use test-only BCrypt salt)
- SQL Server connection strings in environment variables (`.env.test`)

---

## Automation Strategy

### Automation Candidates
- All unit tests (100% automated)
- All integration/API tests (100% automated)
- RBAC enforcement tests (automated per endpoint)
- Contract validation (automated schema checks)

### Manual Testing
- Exploratory testing for usability edge cases
- Security penetration testing (Phase 1 completion)
- Cross-browser compatibility (deferred to Frontend phase)

### Automation Framework Stack

| Layer | Tool | Language |
|-------|------|----------|
| Unit | xUnit | C# (.NET 10) |
| Integration | xUnit + WebApplicationFactory | C# (.NET 10) |
| API | xUnit + HttpClient | C# (.NET 10) |
| E2E (Phase 2) | Playwright or Cypress | TypeScript |

---

## CI/CD Integration

### Pipeline Stages

1. **Pre-commit:** Linting (dotnet format), fast unit tests
2. **PR:** Full unit test suite + integration tests
3. **Merge to main:** Full test suite + contract validation
4. **Nightly:** Regression suite (if implemented)
5. **Pre-release:** Full suite + security scan + deployment validation

### Quality Gates

| Gate | Criteria | Blocking |
|------|----------|----------|
| Unit coverage | >= 90% | Yes |
| Unit tests | 100% pass | Yes |
| Integration tests | 100% pass | Yes |
| Security tests | 100% pass | Yes |
| Linting | Zero errors | Yes |

**Merge criteria:** All blocking gates must pass before PR approval.

---

## Defect Management

### Severity Definitions

| Severity | Definition | SLA |
|----------|------------|-----|
| Critical | Authentication broken, data loss, system crash | Immediate fix |
| High | Major feature broken (e.g., CRUD endpoint fails), no workaround | 1 business day |
| Medium | Feature impaired, workaround exists | 1 week |
| Low | Minor issue, cosmetic | Backlog |

### Bug Workflow
1. Bug reported via `/sdlc-studio bug` → creates `sdlc-studio/bugs/BG{NNNN}.md`
2. Triaged by severity
3. Linked to Story or Epic
4. Fixed with test regression added
5. Verified and closed

---

## Tools & Infrastructure

| Purpose | Tool |
|---------|------|
| Test Framework | xUnit (.NET 10) |
| Mocking | Moq |
| Coverage | `dotnet test --collect:"XPlat Code Coverage"` |
| CI/CD | GitHub Actions (TBD) or Azure DevOps |
| Container Testing | Testcontainers (SQL Server) |
| API Documentation | Swagger/OpenAPI |

---

## Test Organisation

```text
DSB-PoC/
  DSB - DXP+/
    tests/
      Login.Api.Tests/
        Features/
          Auth/
            LoginCommandHandlerTests.cs
            RefreshTokenHandlerTests.cs
        Integration/
          AuthEndpointTests.cs
      Management.Api.Tests/
        Features/
          Users/
            CreateUserHandlerTests.cs
            GetUserHandlerTests.cs
          Workshops/
            CreateWorkshopHandlerTests.cs
          Advisors/
            CreateAdvisorHandlerTests.cs
        Integration/
          UserEndpointTests.cs
          WorkshopEndpointTests.cs
          AdvisorEndpointTests.cs
      Fixtures/
        TestUsers.json
        TestWorkshops.json
```

---

## Test Anti-Patterns to Avoid

### 1. Conditional Assertions
❌ **BAD:**
```csharp
if (users.Count > 0) {
    Assert.Equal("Admin", users[0].Role);
}
```
✅ **GOOD:**
```csharp
Assert.NotEmpty(users);
Assert.Equal("Admin", users[0].Role);
```

### 2. Testing DTOs Instead of Behavior
❌ **BAD:** Testing getters/setters
✅ **GOOD:** Testing command handlers and validation logic

### 3. Ignoring Async/Await
❌ **BAD:** `handler.Handle(command)` (missing await)
✅ **GOOD:** `await handler.Handle(command, CancellationToken.None)`

---

## Related Specifications

- [Product Requirements Document](prd.md)
- [Technical Requirements Document](trd.md)
- [User Personas](personas.md)
- [Epic Registry](epics/_index.md)
- [Story Registry](stories/_index.md)

---

## Revision History

| Date | Author | Change |
|------|--------|--------|
| 2026-05-29 | AI Assistant | Initial TSD creation for Phase 1 (Login.Api + Management.Api) |
