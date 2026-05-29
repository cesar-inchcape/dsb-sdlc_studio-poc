# Authorization & Role-Based Access Control (RBAC) Guide

## Overview

This document describes how authorization is implemented in the DSB Login API, including role-based policies, endpoint protection, and examples for adding authorization to new endpoints.

## Authentication vs Authorization

- **Authentication (US0001):** Verifies the user's identity via JWT tokens
- **Authorization (US0002):** Verifies what the authenticated user is allowed to do based on roles

## Role Hierarchy

The system implements a hierarchical role structure:

```
SuperAdmin (Platform-wide control)
  ├─ Can: Access all APIs, manage all users, modify system configuration
  └─ Policy: SuperAdminOnly

DistributorAdmin (Brand/Region-scoped control)
  ├─ Can: Manage workshops & advisors for assigned brands/regions
  ├─ Cannot: Access other distributors' resources
  └─ Policy: AdminOrHigher

RetailGroupUser (Limited access)
  ├─ Can: View and book service appointments
  ├─ Cannot: Modify workshop/advisor configurations
  └─ Policy: AdminOrHigher (if needed)

WorkshopUser (Read-only access)
  ├─ Can: View workshop data (read-only)
  ├─ Cannot: Write operations on any resource
  └─ Policy: WorkshopReadOnly
```

## Authorization Policies

Three built-in policies are available:

### 1. SuperAdminOnly
**Requirement:** User must have `SuperAdmin` role

**Usage:**
```csharp
.RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)

// Or on methods:
[Authorize(Policy = AuthorizationPolicies.SuperAdminOnly)]
```

**Allowed Roles:** SuperAdmin
**HTTP Response on Denial:** 403 Forbidden

**Example Endpoints:**
- `GET /api/admin/users` - List all users
- `POST /api/admin/config` - Modify system configuration

---

### 2. AdminOrHigher
**Requirement:** User must have `SuperAdmin` OR `DistributorAdmin` role

**Usage:**
```csharp
.RequireAuthorization(AuthorizationPolicies.AdminOrHigher)

// Or on methods:
[Authorize(Policy = AuthorizationPolicies.AdminOrHigher)]
```

**Allowed Roles:** SuperAdmin, DistributorAdmin
**HTTP Response on Denial:** 403 Forbidden

**Example Endpoints:**
- `GET /api/management/workshops` - List workshops
- `POST /api/management/advisors` - Create advisor
- `PUT /api/management/workshops/{id}` - Update workshop

---

### 3. WorkshopReadOnly
**Requirement:** User must be authenticated (any role)

**Usage:**
```csharp
.RequireAuthorization(AuthorizationPolicies.WorkshopReadOnly)

// Or on methods:
[Authorize(Policy = AuthorizationPolicies.WorkshopReadOnly)]
```

**Allowed Roles:** Any authenticated user (WorkshopUser, DistributorAdmin, SuperAdmin)
**HTTP Response on Denial:** 401 Unauthorized (not authenticated)

**Note:** Write operation enforcement (403 Forbidden) must be implemented separately at the handler level for specific endpoints.

**Example Endpoints:**
- `GET /api/workshop/info` - Read workshop information
- `GET /api/workshop/schedule` - Read workshop schedule

---

## Implementing Authorization on New Endpoints

### Minimal API (Recommended)

```csharp
// Require SuperAdmin role
group.MapGet("/admin-endpoint", () => Results.Ok())
    .RequireAuthorization(AuthorizationPolicies.SuperAdminOnly)
    .WithName("AdminEndpoint")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status403Forbidden)
    .Produces(StatusCodes.Status401Unauthorized);

// Require Admin or higher
group.MapPost("/create-workshop", async (CreateWorkshopRequest req, IMediator mediator) =>
{
    var command = new CreateWorkshopCommand(req.Name, req.Location);
    var result = await mediator.Send(command);
    return Results.CreatedAtRoute(nameof(GetWorkshop), new { id = result.Id }, result);
})
.RequireAuthorization(AuthorizationPolicies.AdminOrHigher)
.WithName("CreateWorkshop");

// Require authentication only
group.MapGet("/workshop-schedule", (HttpContext context) =>
{
    var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
    return Results.Ok(new { email = userEmail });
})
.RequireAuthorization(AuthorizationPolicies.WorkshopReadOnly)
.WithName("GetWorkshopSchedule");
```

### In Program.cs

```csharp
// Endpoints are automatically mapped when calling MapAuthEndpoints()
app.MapAuthEndpoints();
```

## Testing Authorization

### Unit Tests: AuthorizationPolicyTests.cs
Tests policy evaluation logic with different role combinations:
```csharp
[Fact]
public void SuperAdminPolicy_WithSuperAdminRole_AllowsAccess()
{
    var claims = new[] { new Claim(ClaimTypes.Role, "SuperAdmin") };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
    
    principal.IsInRole("SuperAdmin").Should().BeTrue();
}
```

### Unit Tests: RoleEnforcementTests.cs
Tests cross-role access prevention and privilege escalation scenarios:
```csharp
[Fact]
public void DistributorAdmin_CannotAccessSuperAdminOnly()
{
    var claims = new[] { new Claim(ClaimTypes.Role, "DistributorAdmin") };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
    
    principal.IsInRole("SuperAdmin").Should().BeFalse();
}
```

### Integration Tests: AuthorizationEndpointTests.cs
Tests full HTTP request/response cycles with protected endpoints:
```csharp
[Fact]
public async Task Login_PublicEndpoint_AllowsAccessWithoutAuthorization()
{
    var request = new LoginRequest("admin@dsb.cl", "Admin123!");
    var response = await _client.PostAsJsonAsync("/api/auth/login", request);
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task RefreshToken_PublicEndpoint_AllowsAccessWithoutAuthorization()
{
    var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Security Best Practices

### 1. Always Check Authentication First
Protected endpoints return `401 Unauthorized` if user is not authenticated:
```
GET /api/admin/users (no token)
→ 401 Unauthorized
```

### 2. Then Check Authorization
If authenticated but insufficient role, return `403 Forbidden`:
```
GET /api/admin/users (DistributorAdmin token)
→ 403 Forbidden
```

### 3. Generic Error Messages
Never reveal role information in error messages to prevent information leakage:
```csharp
// ❌ Bad: "You need SuperAdmin role to access this"
// ✅ Good: "Access denied"
```

### 4. Principle of Least Privilege
Users should have minimal required roles:
- WorkshopUser: Read-only workshop data
- DistributorAdmin: Manage own brand/region
- SuperAdmin: Full system access (restricted to platform admins)

### 5. Case Sensitivity
Role names are **case-sensitive**:
```csharp
"SuperAdmin"    // ✅ Correct
"superadmin"    // ❌ Will not match
"SUPERADMIN"    // ❌ Will not match
```

### 6. Multiple Roles Per User
A user can have multiple roles. All policies are evaluated:
```csharp
User: [DistributorAdmin, WorkshopUser]
- Can access AdminOrHigher endpoints? ✅ Yes (has DistributorAdmin)
- Can access WorkshopReadOnly endpoints? ✅ Yes (has WorkshopUser)
- Can access SuperAdminOnly endpoints? ❌ No (doesn't have SuperAdmin)
```

## Common Issues & Solutions

### Issue: Always Getting 401 Unauthorized
**Cause:** Token not included in request header
**Solution:**
```csharp
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
```

### Issue: Always Getting 403 Forbidden
**Cause:** User has correct authentication but wrong role
**Solution:** Verify user role in JWT claims and authorization policy

### Issue: Policy Not Applied
**Cause:** `.RequireAuthorization()` not called on endpoint
**Solution:**
```csharp
.RequireAuthorization(AuthorizationPolicies.SuperAdminOnly) // Add this
```

### Issue: Role Not in JWT Token
**Cause:** Role claims not included during token generation
**Solution:** Verify JwtTokenGenerator includes role claims:
```csharp
foreach (var role in roles)
{
    claims.Add(new Claim(ClaimTypes.Role, role));
}
```

## Testing Checklist

- [ ] Public endpoints (login, refresh) work without authentication
- [ ] Protected endpoints return 401 without authentication
- [ ] Protected endpoints return 403 with wrong role
- [ ] Protected endpoints return 200 with correct role
- [ ] SuperAdmin can access all endpoints
- [ ] DistributorAdmin cannot access SuperAdminOnly endpoints
- [ ] WorkshopUser cannot access Admin endpoints
- [ ] Multiple roles on a user work correctly
- [ ] Role names are case-sensitive
- [ ] No information leakage in error messages

## Related Files

- [AuthorizationPolicies.cs](../Infrastructure/Authorization/AuthorizationPolicies.cs) - Policy definitions
- [AuthorizeRoleAttribute.cs](../Infrastructure/Authorization/AuthorizeRoleAttribute.cs) - Custom attribute
- [AuthEndpoints.cs](../Features/Auth/AuthEndpoints.cs) - Protected endpoints example
- [AuthorizationPolicyTests.cs](../../Tests/Infrastructure/Authorization/AuthorizationPolicyTests.cs) - Unit tests
- [RoleEnforcementTests.cs](../../Tests/Infrastructure/Security/RoleEnforcementTests.cs) - Role enforcement tests
- [AuthorizationEndpointTests.cs](../../Tests/Integration/AuthorizationEndpointTests.cs) - Integration tests

## Further Reading

- [Microsoft Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
- [Policy-based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [JWT Authentication Best Practices](https://tools.ietf.org/html/rfc7519)
