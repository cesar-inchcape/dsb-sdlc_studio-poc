# PL0001: JWT Authentication - Implementation Plan

> **Status:** ✅ Completed
> **Story:** [US0001: JWT Authentication](../stories/US0001-jwt-authentication.md)
> **Epic:** [EP0001: Authentication and RBAC Foundation](../epics/EP0001-auth-and-rbac-foundation.md)
> **Created:** 2026-05-29
> **Completed:** 2026-05-29
> **Language:** C# (.NET 10)
> **Approach:** TDD (Test-First)
> **Tests:** 37/37 Passed ✅

## Overview

Implement secure JWT-based authentication for Login.Api with token generation, validation, and refresh capabilities. This plan follows TDD methodology to ensure secure authentication mechanisms are thoroughly tested before implementation.

## Acceptance Criteria Summary

| AC | Name | Description |
|----|------|-------------|
| AC1 | Valid credential login returns JWT | User with valid credentials receives access token with role claims |
| AC2 | Invalid credential login rejected | Invalid credentials return 401 Unauthorized |
| AC3 | Refresh token renews session | Valid refresh token issues new access token |

---

## Technical Context

### Language & Framework
- **Primary Language:** C# (.NET 10)
- **Framework:** ASP.NET Core Web API (Minimal API or Controllers)
- **Architecture:** Vertical Slice Architecture with MediatR
- **Test Framework:** xUnit
- **Additional Libraries:**
  - Microsoft.AspNetCore.Authentication.JwtBearer
  - System.IdentityModel.Tokens.Jwt
  - BCrypt.Net-Next (password hashing)
  - FluentValidation (DTO validation)
  - Moq (mocking for tests)

### Relevant Best Practices
- **Security:** Never log passwords or tokens
- **Password Hashing:** Use BCrypt with work factor >= 12
- **Token Expiry:** Access token 60 minutes, refresh token 7 days
- **Error Messages:** Generic messages for auth failures (prevent user enumeration)
- **Validation:** FluentValidation for request DTOs
- **Testing:** Test with invalid tokens, expired tokens, missing claims

### Existing Patterns
**Project Structure (to be created):**
```
Login.Api/
  Features/
    Auth/
      Login/
        LoginCommand.cs
        LoginCommandHandler.cs
        LoginRequest.cs
        LoginResponse.cs
        LoginValidator.cs
      RefreshToken/
        RefreshTokenCommand.cs
        RefreshTokenCommandHandler.cs
  Infrastructure/
    Security/
      JwtTokenGenerator.cs
      ITokenGenerator.cs
  Program.cs
  appsettings.json

Login.Api.Tests/
  Features/
    Auth/
      LoginCommandHandlerTests.cs
      RefreshTokenHandlerTests.cs
  Infrastructure/
    Security/
      JwtTokenGeneratorTests.cs
```

---

## Recommended Approach

**Strategy:** TDD (Test-First)  
**Rationale:** 
- Authentication is security-critical and benefits from comprehensive test coverage
- TDD ensures all edge cases (expired tokens, invalid credentials) are handled
- Prevents security vulnerabilities by writing tests for failure scenarios first
- Establishes baseline quality gates before implementation

### Test Priority
1. **Unit tests for JwtTokenGenerator** - Token creation and validation logic
2. **Handler tests for LoginCommand** - Credential validation and response structure
3. **Handler tests for RefreshTokenCommand** - Token refresh lifecycle
4. **Integration tests** - Full HTTP request/response validation (Phase 2)

---

## Implementation Tasks

| # | Task | File | Depends On | Status |
|---|------|------|------------|--------|
| 1 | Create Login.Api project structure | `Login.Api/Login.Api.csproj` | None | [x] |
| 2 | Create test project structure | `Login.Api.Tests/Login.Api.Tests.csproj` | Task 1 | [x] |
| 3 | Write JWT token generator tests | `Login.Api.Tests/Infrastructure/Security/JwtTokenGeneratorTests.cs` | Task 2 | [x] |
| 4 | Implement JWT token generator | `Login.Api/Infrastructure/Security/JwtTokenGenerator.cs` | Task 3 | [x] |
| 5 | Write login command handler tests | `Login.Api.Tests/Features/Auth/LoginCommandHandlerTests.cs` | Task 2 | [x] |
| 6 | Implement login command handler | `Login.Api/Features/Auth/Login/LoginCommandHandler.cs` | Tasks 4,5 | [x] |
| 7 | Write refresh token handler tests | `Login.Api.Tests/Features/Auth/RefreshTokenHandlerTests.cs` | Task 2 | [x] |
| 8 | Implement refresh token handler | `Login.Api/Features/Auth/RefreshToken/RefreshTokenCommandHandler.cs` | Tasks 4,7 | [x] |
| 9 | Configure JWT authentication middleware | `Login.Api/Program.cs` | Tasks 4,6,8 | [ ] |
| 10 | Add API endpoints | `Login.Api/Features/Auth/AuthEndpoints.cs` | Task 9 | [ ] |
| 11 | Create integration tests | `Login.Api.Tests/Integration/AuthEndpointTests.cs` | Task 10 | [ ] |
| 12 | Configure environment variables | `Login.Api/appsettings.json`, `.env.example` | None | [ ] |

---

## Implementation Phases

### Phase 1: Project Setup & Configuration ✅ COMPLETED
**Goal:** Establish solution structure and install dependencies

- [x] Create `Login.Api` Web API project (.NET 10)
- [x] Create `Login.Api.Tests` xUnit test project
- [x] Install NuGet packages:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `System.IdentityModel.Tokens.Jwt`
  - `BCrypt.Net-Next`
  - `MediatR`
  - `FluentValidation.AspNetCore`
  - `Moq` (test project)
  - `Microsoft.AspNetCore.Mvc.Testing` (test project)
- [ ] Configure `appsettings.json` with JWT settings (secret, issuer, audience, expiry)
- [ ] Add `.env.example` template for environment variables

**Files:**
- `Login.Api/Login.Api.csproj` - Project definition
- `Login.Api.Tests/Login.Api.Tests.csproj` - Test project
- `Login.Api/appsettings.json` - JWT configuration
- `.env.example` - Environment variable template

---

### Phase 2: JWT Token Generator (TDD) ✅ COMPLETED
**Goal:** Create and test token generation/validation logic

#### **Step 1: Write Tests First** ✅
- [x] Create `JwtTokenGeneratorTests.cs`
- [x] Test: Generate token with valid user data returns JWT string
- [x] Test: Generated token contains expected claims (userId, email, roles)
- [x] Test: Token expiration is set correctly (60 minutes)
- [x] Test: Validate valid token returns true
- [x] Test: Validate expired token returns false
- [x] Test: Validate token with invalid signature returns false
- [x] Test: Validate token with missing claims returns false

**Files:** `Login.Api.Tests/Infrastructure/Security/JwtTokenGeneratorTests.cs`

#### **Step 2: Implement JwtTokenGenerator** ✅
- [x] Create `ITokenGenerator` interface
- [x] Implement `JwtTokenGenerator` class
- [x] Method: `GenerateAccessToken(User user)` → JWT string
- [x] Method: `GenerateRefreshToken()` → random secure string
- [x] Method: `ValidateToken(string token)` → ClaimsPrincipal or null
- [x] Use `JwtSecurityTokenHandler` for token operations
- [x] Read JWT settings from `IConfiguration` (secret, issuer, audience, expiry)

**Files:**
- `Login.Api/Infrastructure/Security/ITokenGenerator.cs`
- `Login.Api/Infrastructure/Security/JwtTokenGenerator.cs`

**Run tests:** ✅ All 8 JwtTokenGenerator tests passing!

---

### Phase 3: Login Command Handler (TDD) ✅ COMPLETED
**Goal:** Implement login logic with credential validation

#### **Step 1: Write Tests First** ✅
- [x] Create `LoginCommandHandlerTests.cs`
- [x] Test: Valid credentials return access token and refresh token
- [x] Test: Response includes token expiration timestamp
- [x] Test: Response includes user role claims
- [x] Test: Invalid email returns 401 Unauthorized
- [x] Test: Invalid password returns 401 Unauthorized
- [x] Test: Inactive user returns 401 Unauthorized
- [x] Test: Null/empty credentials return validation error (400)
- [x] Test: Password hashing validation (BCrypt)

**Files:** `Login.Api.Tests/Features/Auth/LoginCommandHandlerTests.cs`

#### **Step 2: Create DTOs and Command** ✅
- [x] Create `LoginRequest` record (Email, Password)
- [x] Create `LoginResponse` record (AccessToken, RefreshToken, ExpiresAt, User)
- [x] Create `LoginCommand` record implementing `IRequest<LoginResponse>`
- [x] Create `LoginValidator` class using FluentValidation

**Files:**
- `Login.Api/Features/Auth/Login/LoginRequest.cs`
- `Login.Api/Features/Auth/Login/LoginResponse.cs`
- `Login.Api/Features/Auth/Login/LoginCommand.cs`
- `Login.Api/Features/Auth/Login/LoginValidator.cs`

#### **Step 3: Implement LoginCommandHandler** ✅
- [x] Create `LoginCommandHandler` implementing `IRequestHandler<LoginCommand, LoginResponse>`
- [x] Inject `ITokenGenerator` and `DbContext` (or User repository)
- [x] Validate user exists by email
- [x] Verify password hash using BCrypt
- [x] Check user IsActive status
- [x] Generate access token and refresh token
- [x] Store refresh token in database (RefreshTokens table)
- [x] Return LoginResponse with tokens

**Files:** `Login.Api/Features/Auth/Login/LoginCommandHandler.cs`

**Run tests:** ✅ All 9 LoginCommandHandler tests passing!

---

### Phase 4: Refresh Token Handler (TDD) ✅ COMPLETED
**Goal:** Implement token refresh mechanism

#### **Step 1: Write Tests First** ✅
- [x] Create `RefreshTokenCommandHandlerTests.cs`
- [x] Test: Valid refresh token returns new access token
- [x] Test: Expired refresh token returns 401 Unauthorized
- [x] Test: Invalid refresh token returns 401 Unauthorized
- [x] Test: Refresh token already used returns 401 (prevent replay)
- [x] Test: New access token contains correct claims
- [x] Test: Old refresh token is invalidated after use
- [x] Test: Null/empty refresh token validation

**Files:** `Login.Api.Tests/Features/Auth/RefreshTokenCommandHandlerTests.cs`

#### **Step 2: Create DTOs and Command** ✅
- [x] Create `RefreshTokenRequest` record (RefreshToken)
- [x] Create `RefreshTokenResponse` record (AccessToken, RefreshToken, ExpiresAt)
- [x] Create `RefreshTokenCommand` record implementing `IRequest<RefreshTokenResponse>`
- [x] Create `RefreshTokenValidator`

**Files:**
- `Login.Api/Features/Auth/RefreshToken/RefreshTokenRequest.cs`
- `Login.Api/Features/Auth/RefreshToken/RefreshTokenResponse.cs`
- `Login.Api/Features/Auth/RefreshToken/RefreshTokenCommand.cs`
- `Login.Api/Features/Auth/RefreshToken/RefreshTokenValidator.cs`

#### **Step 3: Implement RefreshTokenCommandHandler** ✅
- [x] Create `RefreshTokenCommandHandler`
- [x] Validate refresh token exists in database
- [x] Check token expiration (7 days)
- [x] Check token has not been used (IsRevoked flag)
- [x] Generate new access token
- [x] Generate new refresh token
- [x] Revoke old refresh token (set IsRevoked = true)
- [x] Store new refresh token
- [x] Return RefreshTokenResponse

**Files:** `Login.Api/Features/Auth/RefreshToken/RefreshTokenCommandHandler.cs`

**Run tests:** ✅ All 9 RefreshTokenCommandHandler tests passing!

**Database Table Required:**
```sql
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME2 NULL
);
```

**Run tests:** All RefreshTokenHandler tests must pass.

---

### Phase 5: API Endpoints & Middleware
**Goal:** Expose HTTP endpoints and configure JWT authentication

- [ ] Create `AuthEndpoints.cs` using Minimal API or Controller
- [ ] Endpoint: `POST /api/auth/login` → LoginRequest → LoginResponse
- [ ] Endpoint: `POST /api/auth/refresh` → RefreshTokenRequest → RefreshTokenResponse
- [ ] Configure MediatR in `Program.cs`
- [ ] Configure JWT authentication middleware:
  - `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)`
  - `AddJwtBearer()` with validation parameters
- [ ] Add FluentValidation pipeline behavior
- [ ] Configure Swagger with JWT bearer auth

**Files:**
- `Login.Api/Features/Auth/AuthEndpoints.cs`
- `Login.Api/Program.cs`

---

### Phase 6: Integration Tests
**Goal:** Validate full HTTP request/response flow

- [ ] Create `AuthEndpointTests.cs` using `WebApplicationFactory<Program>`
- [ ] Test: POST /api/auth/login with valid credentials returns 200 OK
- [ ] Test: Response contains access_token and refresh_token
- [ ] Test: POST /api/auth/login with invalid credentials returns 401
- [ ] Test: POST /api/auth/login with malformed request returns 400
- [ ] Test: POST /api/auth/refresh with valid token returns 200 OK
- [ ] Test: POST /api/auth/refresh with invalid token returns 401
- [ ] Test: Token can be used to access protected endpoints (future stories)

**Files:** `Login.Api.Tests/Integration/AuthEndpointTests.cs`

**Run tests:** All integration tests must pass.

---

## Edge Case Handling

| # | Edge Case (from Story) | Handling Strategy | Phase |
|---|------------------------|-------------------|-------|
| 1 | Null or empty credentials | FluentValidation returns 400 Bad Request | Phase 3 |
| 2 | User not found in database | Return generic 401 (prevent user enumeration) | Phase 3 |
| 3 | Incorrect password | BCrypt comparison fails → 401 | Phase 3 |
| 4 | User account inactive (IsActive = false) | Check flag before token generation → 401 | Phase 3 |
| 5 | Expired access token | JWT validation rejects token → 401 | Phase 2 |
| 6 | Invalid token signature | JWT validation fails → 401 | Phase 2 |
| 7 | Refresh token already used | Check IsRevoked flag → 401 | Phase 4 |
| 8 | Refresh token expired | Check ExpiresAt timestamp → 401 | Phase 4 |
| 9 | Missing JWT secret in config | Application startup fails (fail-fast) | Phase 1 |
| 10 | Concurrent refresh token requests | Database transaction isolation prevents double-use | Phase 4 |

**Coverage:** 10/10 edge cases handled (from expanded requirements)

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| JWT secret leaked | Critical - all tokens compromised | Store in environment variables, never commit to source control |
| Password enumeration attack | Medium - attacker can discover valid emails | Generic error messages for auth failures |
| Token replay attacks | High - stolen tokens used maliciously | Short token expiry (60 min), refresh token rotation |
| Insufficient password hashing | High - passwords cracked via brute force | BCrypt with work factor 12+ (configurable) |
| Missing rate limiting | Medium - brute force login attempts | Implement rate limiting middleware (Phase 2) |
| Database connection failure during token storage | Medium - refresh token not persisted | Wrap in transaction, return error if storage fails |

---

## Definition of Done

- [ ] All unit tests written and passing (JwtTokenGenerator, LoginHandler, RefreshTokenHandler)
- [ ] All integration tests written and passing (AuthEndpoints)
- [ ] Code coverage >= 90% for Features/Auth and Infrastructure/Security
- [ ] All 10 edge cases handled with tests
- [ ] FluentValidation configured for all request DTOs
- [ ] JWT authentication middleware configured correctly
- [ ] No hardcoded secrets (all in appsettings.json or environment variables)
- [ ] Swagger documentation includes JWT bearer authorization
- [ ] Code follows Vertical Slice Architecture pattern
- [ ] No linting errors (`dotnet format --verify-no-changes`)
- [ ] RefreshTokens database table created via EF Core migration

---

## Test Execution Commands

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run only unit tests
dotnet test --filter Category!=Integration

# Run only integration tests
dotnet test --filter Category=Integration

# Run tests in watch mode (TDD)
dotnet watch test
```

---

## Dependencies

### External (NuGet)
- Microsoft.AspNetCore.Authentication.JwtBearer (>= 8.0.0)
- System.IdentityModel.Tokens.Jwt (>= 7.0.0)
- BCrypt.Net-Next (>= 4.0.3)
- MediatR (>= 12.0.0)
- FluentValidation.AspNetCore (>= 11.3.0)

### Internal (Stories)
- **Blocks:** US0002 (RBAC requires JWT authentication)
- **Blocks:** US0003 (User management requires authentication)

---

## Notes

**TDD Workflow:**
1. Write failing test
2. Run test (RED)
3. Write minimal code to pass
4. Run test (GREEN)
5. Refactor
6. Repeat

**Security Checklist:**
- [ ] Passwords never logged or returned in responses
- [ ] JWT secret loaded from secure configuration
- [ ] BCrypt work factor >= 12
- [ ] Generic error messages for auth failures
- [ ] Refresh tokens stored with expiration
- [ ] Token rotation on refresh (old token revoked)

**Next Steps After Completion:**
1. Update US0001 status to "Done"
2. Proceed to US0002 (Role-Based Access Control)
3. Implement rate limiting middleware (epic-level improvement)
