# PL0002: Role-Based Access Control - Implementation Plan

> **Status:** ✅ Completed
> **Story:** [US0002: Role-Based Access Control](../stories/US0002-role-based-access-control.md)
> **Epic:** [EP0001: Authentication and RBAC Foundation](../epics/EP0001-auth-and-rbac-foundation.md)
> **Created:** 2026-05-29
> **Completed:** 2026-05-29
> **Language:** C# (.NET 10)
> **Approach:** TDD (Test-First)
> **Tests:** 24/24 Passed ✅ (61 total with US0001)

## Overview

Implement authorization middleware and role-based policy enforcement for Login.Api. This builds on JWT authentication (US0001) to restrict endpoint access based on user roles and permissions.

## Acceptance Criteria Summary

| AC | Name | Description |
|----|------|-------------|
| AC1 | Role claims enforced by middleware | Protected endpoints validate role claims; authorization succeeds only if policy allows |
| AC2 | Unauthorized role access blocked | Users without required roles receive 403 Forbidden |
| AC3 | Read-only enforcement | Workshop Users cannot write; read-only paths remain accessible |

---

## Technical Context

### Language & Framework
- **Primary Language:** C# (.NET 10)
- **Framework:** ASP.NET Core Web API
- **Authentication:** JWT Bearer (from US0001)
- **Authorization:** Policy-Based Authorization
- **Test Framework:** xUnit
- **Additional Libraries:**
  - Microsoft.AspNetCore.Authorization
  - FluentAssertions (tests)
  - Moq (test mocking)

### Role Hierarchy
```
SuperAdmin
  └─ Can: All operations, manage all users/workshops/advisors

DistributorAdmin
  └─ Can: Manage workshops & advisors for assigned brands/regions
  └─ Cannot: Manage other distributors' resources

RetailGroupUser
  └─ Can: View and book service appointments
  └─ Cannot: Modify workshop/advisor configurations

WorkshopUser
  └─ Can: View workshop data (read-only)
  └─ Cannot: Write operations on any resource
```

### Existing Patterns from US0001
- JWT token contains role claims (`ClaimTypes.Role`)
- LoginDbContext has User, Role, UserRole entities
- Vertical Slice Architecture with MediatR
- TDD approach with unit + integration tests

---

## Recommended Approach

**Strategy:** TDD (Test-First)  
**Rationale:**
- Authorization is security-critical; comprehensive tests prevent vulnerabilities
- TDD ensures edge cases (cross-role access, missing claims) are handled
- Establishes clear policy contracts before middleware implementation
- Enables rapid iteration on role policies

### Test Priority
1. **Unit tests for Authorization Policies** - Policy evaluation logic
2. **Handler tests with authorization** - Verify handlers check roles
3. **Endpoint integration tests** - Full HTTP 403/200 scenarios
4. **Cross-role access tests** - Prevent privilege escalation

---

## Implementation Tasks

| # | Task | File | Depends On | Status |
|---|------|------|------------|--------|
| 1 | Write authorization policy tests | `Login.Api.Tests/Infrastructure/Authorization/AuthorizationPolicyTests.cs` | None | [ ] |
| 2 | Implement authorization policies | `Login.Api/Infrastructure/Authorization/AuthorizationPolicies.cs` | Task 1 | [ ] |
| 3 | Create authorization attributes | `Login.Api/Infrastructure/Authorization/AuthorizeRoleAttribute.cs` | Task 2 | [ ] |
| 4 | Write endpoint authorization tests | `Login.Api.Tests/Integration/AuthorizationEndpointTests.cs` | Task 3 | [ ] |
| 5 | Configure policies in Program.cs | `Login.Api/Program.cs` | Tasks 2,4 | [ ] |
| 6 | Add [Authorize] attributes to endpoints | `Login.Api/Features/Auth/AuthEndpoints.cs` | Task 5 | [ ] |
| 7 | Write tests for role enforcement | `Login.Api.Tests/Infrastructure/Authorization/RoleEnforcementTests.cs` | Task 2 | [ ] |
| 8 | Document authorization patterns | `Login.Api/README-AUTHORIZATION.md` | Tasks 2,3,6 | [ ] |

---

## Implementation Phases

### Phase 1: Authorization Infrastructure (Days 1-2)
**Goal:** Create authorization policy framework and test infrastructure

#### Step 1: Write Authorization Policy Tests
- [ ] Create `AuthorizationPolicies.cs` (test file)
- [ ] Test: SuperAdmin policy allows any role
- [ ] Test: DistributorAdmin policy restricts to DistributorAdmin + SuperAdmin
- [ ] Test: WorkshopUser policy enforces read-only
- [ ] Test: Invalid role policy returns 403

#### Step 2: Implement Authorization Policies
- [ ] Create authorization policies registration in Program.cs
- [ ] Define policy: `SuperAdminOnly` - requires SuperAdmin role
- [ ] Define policy: `AdminOrHigher` - requires DistributorAdmin or SuperAdmin
- [ ] Define policy: `WorkshopReadOnly` - allows read; blocks write
- [ ] Policy evaluation with role claims from JWT

#### Step 3: Create Authorization Attributes
- [ ] Custom `[AuthorizeRole(...)]` attribute for endpoints
- [ ] Integrates with standard `[Authorize]` middleware
- [ ] Supports multiple role specifications
- [ ] Returns 403 Forbidden on role mismatch

**Files Created:**
- `Login.Api/Infrastructure/Authorization/AuthorizationPolicies.cs`
- `Login.Api/Infrastructure/Authorization/AuthorizeRoleAttribute.cs`

**Tests:**
- `Login.Api.Tests/Infrastructure/Authorization/AuthorizationPolicyTests.cs` (6-8 tests)

---

### Phase 2: Endpoint Protection (Days 2-3)
**Goal:** Apply authorization to existing endpoints and test

#### Step 4: Write Integration Tests for Authorization
- [ ] Test: POST /api/auth/login without [Authorize] (public endpoint)
- [ ] Test: POST /api/auth/refresh without [Authorize] (public endpoint)
- [ ] Create protected test endpoint to verify [Authorize] middleware
- [ ] Test: Authorized user gets 200; unauthorized gets 403
- [ ] Test: Missing/invalid JWT gets 401 Unauthorized
- [ ] Test: Valid JWT but insufficient role gets 403 Forbidden

#### Step 5: Configure Policies in Program.cs
- [ ] Add `builder.Services.AddAuthorization(options => { ... })`
- [ ] Register all policies (SuperAdminOnly, AdminOrHigher, WorkshopReadOnly)
- [ ] Configure default challenge/forbid handlers
- [ ] Enable authorization middleware: `app.UseAuthorization()`

#### Step 6: Apply [Authorize] to Endpoints
- [ ] POST /api/auth/login - No authorization (public)
- [ ] POST /api/auth/refresh - No authorization (public)
- [ ] (Placeholder) GET /api/users - [Authorize(Policy = "AdminOrHigher")]
- [ ] (Placeholder) PUT /api/users - [Authorize(Policy = "SuperAdminOnly")]
- [ ] Workshop endpoints - [Authorize(Policy = "WorkshopReadOnly")] for GET

**Files Modified:**
- `Login.Api/Program.cs` - Add authorization policies
- `Login.Api/Features/Auth/AuthEndpoints.cs` - Add [Authorize] attributes

**Tests:**
- `Login.Api.Tests/Integration/AuthorizationEndpointTests.cs` (8-10 tests)

---

### Phase 3: Role Enforcement & Edge Cases (Days 3-4)
**Goal:** Ensure cross-role access prevention and permission boundaries

#### Step 7: Write Role Enforcement Tests
- [ ] Test: SuperAdmin can access all endpoints
- [ ] Test: DistributorAdmin cannot access SuperAdmin-only endpoints
- [ ] Test: WorkshopUser gets 403 on admin endpoints
- [ ] Test: User with no roles gets 403
- [ ] Test: Multiple roles in JWT are evaluated correctly
- [ ] Test: Revoked/inactive users are denied (from auth context)

#### Step 8: Documentation
- [ ] Create `README-AUTHORIZATION.md` with:
  - Policy reference
  - How to add [Authorize] to new endpoints
  - Role hierarchy diagram
  - Common authorization patterns
  - Testing patterns for authorization

**Files:**
- `Login.Api.Tests/Infrastructure/Authorization/RoleEnforcementTests.cs` (5-7 tests)
- `Login.Api/README-AUTHORIZATION.md`

---

## Testing Strategy

### Unit Tests (15-18 tests)
**AuthorizationPolicyTests.cs:**
- Policy evaluation with different role sets
- Policy fallback behavior
- Missing role claims handling

**RoleEnforcementTests.cs:**
- Cross-role access prevention
- Role hierarchy enforcement
- Edge cases (null claims, empty roles)

### Integration Tests (8-10 tests)
**AuthorizationEndpointTests.cs:**
- Protected endpoints with valid auth → 200
- Protected endpoints without auth → 401
- Protected endpoints with wrong role → 403
- Public endpoints (login/refresh) → 200 regardless of auth
- Multiple role validation

### Total Test Coverage Goal
- **Authentication:** 37 tests (from US0001) ✅
- **Authorization:** 23-28 tests (this story)
- **Combined:** 60+ tests
- **Target Coverage:** 95%+ for Auth module

---

## Dependencies & Prerequisites

✅ **Completed (from US0001):**
- JWT authentication middleware
- Token generation with role claims
- User + Role database entities
- LoginDbContext with role relationships

⏳ **Required Before Implementation:**
- None - can start immediately

---

## Success Criteria

- [ ] All 23+ authorization tests passing
- [ ] [Authorize] policies configured in Program.cs
- [ ] Endpoints correctly decorated with [Authorize(Policy = "...")]
- [ ] 403 Forbidden returned for insufficient roles
- [ ] 401 Unauthorized returned for missing auth
- [ ] Public endpoints (login/refresh) remain accessible without auth
- [ ] Role hierarchy enforced (no privilege escalation)
- [ ] Documentation complete with examples

---

## Timeline Estimate

| Phase | Tasks | Estimate | Notes |
|-------|-------|----------|-------|
| **Phase 1** | 1-3 | 4-6 hours | Infrastructure setup |
| **Phase 2** | 4-6 | 4-5 hours | Integration & endpoint protection |
| **Phase 3** | 7-8 | 3-4 hours | Edge cases & documentation |
| **Total** | 8 tasks | **11-15 hours** | Conservative estimate (includes testing) |

---

## Next Steps After Completion

1. **Code Review:** Peer review authorization logic
2. **Security Review:** Verify no privilege escalation paths
3. **US0003 Planning:** User Management CRUD (depends on RBAC)
4. **Documentation:** Update API docs with authorization requirements
5. **Testing:** Run full integration test suite with multiple role scenarios

---

## Related Documentation

- [JWT Authentication Plan](PL0001-jwt-authentication.md)
- [US0001: JWT Authentication](../stories/US0001-jwt-authentication.md)
- [US0002: Role-Based Access Control](../stories/US0002-role-based-access-control.md)
- [PRD: Authentication & Authorization](../prd.md#41-authentication--authorization-loginapi)
