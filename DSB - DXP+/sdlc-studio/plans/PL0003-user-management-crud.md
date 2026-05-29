# PL0003: User Management CRUD & Role Assignment - Implementation Plan

> **Status:** Draft
> **Story:** [US0003: User Management CRUD and Role Assignment](../stories/US0003-user-management-crud-and-role-assignment.md)
> **Epic:** [EP0002: Management API Core Entities](../epics/EP0002-management-api-core-entities.md)
> **Created:** 2026-05-29
> **Language:** C# (.NET 10)
> **Approach:** TDD (Test-First)

## Overview

Implement comprehensive User Management CRUD (Create, Read, Update, Delete) and role assignment operations for the Management.Api microservice. This story builds on JWT authentication (US0001) and RBAC (US0002) to provide user lifecycle management.

## Acceptance Criteria Summary

| AC | Name | Description |
|----|------|-------------|
| AC1 | User CRUD endpoints available & validated | Create, read, update, deactivate users with validation |
| AC2 | Role assignment enforced & auditable | Roles assigned/changed with traceability |
| AC3 | Scope boundaries respected | Distributor Admins cannot access out-of-scope users |

---

## Technical Context

### Language & Framework
- **Primary Language:** C# (.NET 10)
- **Framework:** ASP.NET Core Web API
- **Architecture:** Vertical Slice Architecture with MediatR
- **Test Framework:** xUnit
- **Additional Libraries:**
  - FluentValidation
  - EntityFrameworkCore
  - AutoMapper (optional for DTOs)

### API Endpoints (To Be Implemented)

#### Public Endpoints
- None (all user management requires authentication)

#### Admin Endpoints (SuperAdminOnly)
- `POST /api/admin/users` - Create new user
- `GET /api/admin/users` - List all users (paginated)
- `GET /api/admin/users/{id}` - Get user by ID
- `PUT /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Deactivate user
- `POST /api/admin/users/{id}/roles` - Assign roles
- `DELETE /api/admin/users/{id}/roles/{roleId}` - Remove role

#### Management Endpoints (AdminOrHigher - with scope boundaries)
- `GET /api/management/users/me` - Get current user profile
- `PUT /api/management/users/me` - Update own profile

### Database Context (Existing)
- ✅ User entity exists
- ✅ Role entity exists
- ✅ UserRole relationship exists
- ✅ LoginDbContext configured
- ⏳ May need to extend with audit fields (CreatedBy, UpdatedBy)

---

## Recommended Approach

**Strategy:** TDD (Test-First)  
**Rationale:**
- CRUD operations have well-defined contracts (input validation, output format)
- Authorization layer is already tested; focus on business logic
- User management is critical for platform security

### Test Priority
1. **Unit tests for CreateUserCommand** - Validation, role assignment
2. **Unit tests for UpdateUserCommand** - Scope boundaries
3. **Unit tests for GetUserQuery** - Authorization, filtering
4. **Integration tests for endpoints** - HTTP contracts
5. **Cross-role access tests** - Privilege boundaries

---

## Implementation Tasks

| # | Task | File | Depends On | Effort | Status |
|---|------|------|------------|--------|--------|
| 1 | Write CreateUserCommand tests | `Login.Api.Tests/Features/Users/CreateUserCommandTests.cs` | None | 2h | [ ] |
| 2 | Implement CreateUserCommand & handler | `Login.Api/Features/Users/Create/` | Task 1 | 2h | [ ] |
| 3 | Write UpdateUserCommand tests | `Login.Api.Tests/Features/Users/UpdateUserCommandTests.cs` | None | 2h | [ ] |
| 4 | Implement UpdateUserCommand & handler | `Login.Api/Features/Users/Update/` | Task 3 | 2h | [ ] |
| 5 | Write GetUserQuery tests | `Login.Api.Tests/Features/Users/GetUserQueryTests.cs` | None | 2h | [ ] |
| 6 | Implement GetUserQuery & handler | `Login.Api/Features/Users/Get/` | Task 5 | 2h | [ ] |
| 7 | Write role assignment command tests | `Login.Api.Tests/Features/Users/AssignRoleCommandTests.cs` | Tasks 1-2 | 2h | [ ] |
| 8 | Implement role assignment | `Login.Api/Features/Users/AssignRole/` | Task 7 | 1.5h | [ ] |
| 9 | Create user endpoints | `Login.Api/Features/Users/UserEndpoints.cs` | Tasks 2,4,6,8 | 2h | [ ] |
| 10 | Write endpoint integration tests | `Login.Api.Tests/Integration/UserEndpointTests.cs` | Task 9 | 3h | [ ] |
| 11 | Implement user validators | `Login.Api/Features/Users/*/Validators/` | Task 2 | 1.5h | [ ] |
| 12 | Create DTOs & responses | `Login.Api/Features/Users/*/` | Task 2 | 1h | [ ] |
| 13 | Add audit trail logging | `Login.Api/Features/Users/` | Task 9 | 2h | [ ] |
| 14 | Documentation & examples | `Login.Api/README-USER-MANAGEMENT.md` | Task 13 | 1h | [ ] |

**Total Effort:** ~27.5 hours (conservative estimate)

---

## Implementation Phases

### Phase 1: Command Infrastructure (Days 1-2)
**Goal:** Implement create, update, delete user commands with tests

#### Step 1-2: CreateUserCommand (4 hours)
- [x] Write tests for password validation (min length, complexity)
- [x] Write tests for email uniqueness validation
- [x] Write tests for role assignment during creation
- [x] Implement CreateUserCommand, CreateUserRequest, CreateUserResponse
- [x] Implement handler with BCrypt password hashing
- [x] Implement CreateUserValidator

**Files:**
- `Login.Api.Tests/Features/Users/CreateUserCommandTests.cs`
- `Login.Api/Features/Users/Create/CreateUserCommand.cs`
- `Login.Api/Features/Users/Create/CreateUserCommandHandler.cs`
- `Login.Api/Features/Users/Create/CreateUserRequest.cs`
- `Login.Api/Features/Users/Create/CreateUserResponse.cs`
- `Login.Api/Features/Users/Create/CreateUserValidator.cs`

#### Step 3-4: UpdateUserCommand (4 hours)
- [ ] Write tests for update validation (email uniqueness, scope)
- [ ] Write tests for distributors admins can only update own users
- [ ] Write tests for password reset/change
- [ ] Implement UpdateUserCommand, request, response, handler
- [ ] Implement scope boundary validation

**Files:**
- `Login.Api.Tests/Features/Users/UpdateUserCommandTests.cs`
- `Login.Api/Features/Users/Update/UpdateUserCommand.cs`
- `Login.Api/Features/Users/Update/UpdateUserCommandHandler.cs`
- `Login.Api/Features/Users/Update/UpdateUserRequest.cs`
- `Login.Api/Features/Users/Update/UpdateUserResponse.cs`
- `Login.Api/Features/Users/Update/UpdateUserValidator.cs`

#### Step 7-8: Role Assignment (4 hours)
- [ ] Write tests for role assignment validation
- [ ] Write tests for privilege escalation prevention
- [ ] Implement AssignRoleCommand and handler
- [ ] Implement RemoveRoleCommand and handler
- [ ] Log role changes for audit trail

**Files:**
- `Login.Api.Tests/Features/Users/AssignRoleCommandTests.cs`
- `Login.Api/Features/Users/AssignRole/AssignRoleCommand.cs`
- `Login.Api/Features/Users/AssignRole/AssignRoleCommandHandler.cs`
- `Login.Api/Features/Users/AssignRole/RemoveRoleCommand.cs`

---

### Phase 2: Query Infrastructure (Days 2-3)
**Goal:** Implement read operations with authorization

#### Step 5-6: GetUserQuery & Queries (3 hours)
- [ ] Write tests for user retrieval with role claims
- [ ] Write tests for list endpoint pagination
- [ ] Write tests for current user endpoint
- [ ] Implement GetUserQuery, GetUsersQuery handlers
- [ ] Implement scope filtering for DistributorAdmin

**Files:**
- `Login.Api.Tests/Features/Users/GetUserQueryTests.cs`
- `Login.Api/Features/Users/Get/GetUserQuery.cs`
- `Login.Api/Features/Users/Get/GetUserQueryHandler.cs`
- `Login.Api/Features/Users/Get/GetUsersQuery.cs`
- `Login.Api/Features/Users/Get/GetUsersQueryHandler.cs`
- `Login.Api/Features/Users/Get/UserDto.cs`

---

### Phase 3: Endpoints & Integration (Days 3-4)
**Goal:** Expose operations via REST endpoints and test end-to-end

#### Step 9-10: User Endpoints & Tests (5 hours)
- [ ] Create endpoint group `/api/admin/users` (SuperAdminOnly)
- [ ] Implement POST, GET (single & list), PUT, DELETE endpoints
- [ ] Create endpoint group `/api/management/users` (AdminOrHigher)
- [ ] Implement current user endpoint (GET `/api/management/users/me`)
- [ ] Write comprehensive integration tests
- [ ] Test authorization boundaries

**Files:**
- `Login.Api/Features/Users/UserEndpoints.cs`
- `Login.Api.Tests/Integration/UserEndpointTests.cs`

#### Step 11-13: Validators & Audit (4 hours)
- [ ] Implement FluentValidation validators
- [ ] Add audit logging (who changed what, when)
- [ ] Add exception handling (duplicate emails, invalid roles)
- [ ] Create user response DTOs

---

### Phase 4: Documentation (Day 4)
**Goal:** Comprehensive API documentation

#### Step 14: Documentation (1 hour)
- [ ] Create `README-USER-MANAGEMENT.md`
- [ ] Document all endpoints with examples
- [ ] Document validation rules
- [ ] Provide curl examples for testing
- [ ] Document authorization boundaries

**Files:**
- `Login.Api/README-USER-MANAGEMENT.md`

---

## Testing Strategy

### Unit Tests (18-20 tests)

**CreateUserCommandTests (5 tests):**
```csharp
[Fact] Handle_ValidRequest_CreatesUser()
[Fact] Handle_DuplicateEmail_ThrowsValidationException()
[Fact] Handle_WeakPassword_ThrowsValidationException()
[Fact] Handle_AssignsRolesDuringCreation()
[Fact] Handle_HashesPasswordWithBCrypt()
```

**UpdateUserCommandTests (5 tests):**
```csharp
[Fact] Handle_ValidRequest_UpdatesUser()
[Fact] Handle_ScopeViolation_ThrowsForbiddenException()
[Fact] Handle_DistributorCantUpdateOtherDistributor()
[Fact] Handle_ChangeEmail_ValidatesUniqueness()
[Fact] Handle_InactiveUser_CannotUpdate()
```

**GetUserQueryTests (4 tests):**
```csharp
[Fact] Handle_ExistingUser_ReturnsUserWithRoles()
[Fact] Handle_NonExistentUser_ReturnsNull()
[Fact] Handle_DistributorScoping_FiltersCorrectly()
[Fact] Handle_GetUsers_PaginatesCorrectly()
```

**AssignRoleCommandTests (3-4 tests):**
```csharp
[Fact] Handle_ValidRole_AssignsSuccessfully()
[Fact] Handle_SuperAdminCanAssignAnyRole()
[Fact] Handle_DistributorCantAssignSuperAdmin()
[Fact] Handle_DuplicateRole_DoesNotThrow()
```

### Integration Tests (8-10 tests)

**UserEndpointTests:**
- `POST /api/admin/users` - 200 Created (SuperAdmin) ✅
- `POST /api/admin/users` - 403 Forbidden (DistributorAdmin) ✅
- `GET /api/admin/users` - 200 OK with list (SuperAdmin) ✅
- `PUT /api/admin/users/{id}` - 200 OK (SuperAdmin) ✅
- `DELETE /api/admin/users/{id}` - 204 No Content (SuperAdmin) ✅
- `GET /api/management/users/me` - 200 OK (any authenticated user) ✅
- `POST /api/admin/users/{id}/roles` - 200 OK (SuperAdmin) ✅
- `GET /api/admin/users` (pagination) - Returns paginated results ✅

### Total Coverage Goal
- **Phase 1 (US0001 + US0002):** 61 tests ✅
- **Phase 2 (US0003):** 26-30 tests
- **Combined:** 87-91 tests
- **Target Coverage:** 95%+ for User module

---

## Success Criteria

- [ ] All 26-30 tests passing
- [ ] User CRUD endpoints operational (POST, GET, PUT, DELETE)
- [ ] Role assignment working with audit trail
- [ ] Authorization boundaries enforced (403 responses correct)
- [ ] Email uniqueness validation working
- [ ] Password hashing with BCrypt validated
- [ ] Pagination working on list endpoints
- [ ] Documentation complete with examples
- [ ] No privilege escalation vulnerabilities

---

## Timeline Estimate

| Phase | Tasks | Estimate | Notes |
|-------|-------|----------|-------|
| **Phase 1** | Commands (1-4, 7-8) | 8-10 hours | TDD approach |
| **Phase 2** | Queries (5-6) | 3-4 hours | Building on Phase 1 |
| **Phase 3** | Endpoints (9-10) | 5-6 hours | Integration testing |
| **Phase 4** | Validators, Audit, Docs (11-14) | 4-5 hours | Documentation & polish |
| **Total** | All tasks | **20-25 hours** | Conservative estimate |

---

## Next Steps After Completion

1. **Code Review:** Peer review authorization layer
2. **Security Review:** Verify no privilege escalation paths
3. **US0004 Planning:** Workshop Management CRUD
4. **US0005 Planning:** Advisor Management CRUD
5. **Integration Planning:** Management.Api bootstrap

---

## Related Documentation

- [US0001: JWT Authentication](../stories/US0001-jwt-authentication.md)
- [US0002: Role-Based Access Control](../stories/US0002-role-based-access-control.md)
- [EP0002: Management API Core Entities](../epics/EP0002-management-api-core-entities.md)
- [TRD: Database Schema](../trd.md#41-database-architecture)
- [PRD: User Management Features](../prd.md#32-user-management)
