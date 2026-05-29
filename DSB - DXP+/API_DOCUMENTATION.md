# DSB Login API - Complete Documentation

**Version:** 1.0.0  
**Last Updated:** May 29, 2026  
**Status:** ✅ Complete (175/175 tests passing)

---

## Table of Contents

1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Authorization](#authorization)
4. [Endpoints by Feature](#endpoints-by-feature)
5. [Request/Response Examples](#requestresponse-examples)
6. [Error Handling](#error-handling)
7. [Testing](#testing)

---

## Overview

The DSB Login API is a comprehensive authentication and resource management system built with ASP.NET Core 8.0 using Minimal APIs and CQRS pattern.

### Base URL
```
https://localhost:5001/api
```

### API Versions
- **Current:** v1.0.0
- **Documentation:** Swagger available at `/swagger`

### Key Features
- JWT-based authentication
- Role-based access control (RBAC)
- Resource management (Users, Workshops, Advisors, Schedules)
- Comprehensive input validation
- Soft delete pattern
- Complete test coverage

---

## Authentication

### Login Endpoint

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@dsb.cl",
  "password": "Admin123!"
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
  "expiresAt": "2026-05-29T22:25:00Z",
  "user": {
    "id": "00000000-0000-0000-0000-000000000001",
    "email": "admin@dsb.cl",
    "firstName": "Admin",
    "lastName": "User",
    "isActive": true,
    "roles": ["SuperAdmin"]
  }
}
```

### Refresh Token Endpoint

**Request:**
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440001",
  "expiresAt": "2026-05-29T22:25:00Z"
}
```

### Token Format

JWT tokens include claims:
```json
{
  "sub": "user-uuid",
  "email": "user@dsb.cl",
  "role": ["SuperAdmin"],
  "iat": 1716993900,
  "exp": 1716997500
}
```

**Token Lifetime:** 60 minutes  
**Refresh Token Lifetime:** 7 days

---

## Authorization

### Role Hierarchy

```
SuperAdmin
├── Full platform access
├── Can manage all users, workshops, advisors
└── Can configure schedules and closures

DistributorAdmin
├── Limited to assigned brands/regions
├── Can manage workshop staff
└── Can view assigned workshops

WorkshopUser
└── Read-only access to workshop information
```

### Authorization Policies

| Policy | Allowed Roles | Purpose |
|--------|---------------|---------|
| `SuperAdminOnly` | SuperAdmin | Platform administration |
| `AdminOrHigher` | SuperAdmin, DistributorAdmin | Resource management |
| `WorkshopReadOnly` | All authenticated | Read-only access |

### Adding Authorization to Requests

All requests (except `/login` and `/refresh`) require the Authorization header:

```http
Authorization: Bearer <access_token>
```

---

## Endpoints by Feature

### 1. User Management

#### Create User

**Request:**
```http
POST /api/admin/users
Authorization: Bearer <token>
Content-Type: application/json

{
  "email": "newuser@dsb.cl",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "email": "newuser@dsb.cl",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true,
  "createdAt": "2026-05-29T20:25:00Z"
}
```

#### Get Users (Paginated)

**Request:**
```http
GET /api/admin/users?pageNumber=1&pageSize=10
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "email": "admin@dsb.cl",
      "firstName": "Admin",
      "lastName": "User",
      "isActive": true,
      "createdAt": "2026-05-29T20:00:00Z"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

#### Get Current User

**Request:**
```http
GET /api/management/users/current
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "email": "newuser@dsb.cl",
  "firstName": "John",
  "lastName": "Doe",
  "roles": [],
  "isActive": true
}
```

#### Update User

**Request:**
```http
PUT /api/admin/users/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith"
}
```

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "email": "newuser@dsb.cl",
  "firstName": "Jane",
  "lastName": "Smith",
  "isActive": true
}
```

#### Assign Role

**Request:**
```http
POST /api/admin/users/assign-role
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "roleName": "DistributorAdmin"
}
```

**Response:** `200 OK`
```json
{
  "message": "Role assigned successfully",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "roleName": "DistributorAdmin"
}
```

#### Delete User (Soft Delete)

**Request:**
```http
DELETE /api/admin/users/{id}
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "message": "User deleted successfully"
}
```

---

### 2. Workshop Management

#### Create Workshop

**Request:**
```http
POST /api/admin/workshops
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Concesionario Centro",
  "brand": "Suzuki",
  "location": "Santiago",
  "address": {
    "street": "Av. Providencia 1234",
    "city": "Santiago",
    "region": "Metropolitana",
    "postalCode": "8320000",
    "country": "Chile"
  },
  "capacity": 50
}
```

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "name": "Concesionario Centro",
  "brand": "Suzuki",
  "location": "Santiago",
  "address": {
    "street": "Av. Providencia 1234",
    "city": "Santiago",
    "region": "Metropolitana",
    "postalCode": "8320000",
    "country": "Chile"
  },
  "capacity": 50,
  "isActive": true,
  "createdAt": "2026-05-29T20:25:00Z"
}
```

#### Get Workshops (Admin)

**Request:**
```http
GET /api/admin/workshops?pageNumber=1&pageSize=10&brand=Suzuki&location=Santiago
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "name": "Concesionario Centro",
      "brand": "Suzuki",
      "location": "Santiago",
      "capacity": 50
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

#### Get Workshops (Management)

**Request:**
```http
GET /api/management/workshops
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Concesionario Centro",
    "brand": "Suzuki",
    "location": "Santiago",
    "capacity": 50
  }
]
```

#### Update Workshop

**Request:**
```http
PUT /api/admin/workshops/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Concesionario Centro - Renovado",
  "capacity": 75
}
```

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "name": "Concesionario Centro - Renovado",
  "brand": "Suzuki",
  "location": "Santiago",
  "capacity": 75
}
```

#### Delete Workshop (Soft Delete)

**Request:**
```http
DELETE /api/admin/workshops/{id}
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "message": "Workshop deleted successfully"
}
```

---

### 3. Advisor Management

#### Create Advisor

**Request:**
```http
POST /api/admin/advisors
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Carlos",
  "lastName": "García",
  "email": "carlos.garcia@dsb.cl",
  "phoneNumber": "+56912345678",
  "workshopId": "550e8400-e29b-41d4-a716-446655440002",
  "assignedBrand": "Suzuki",
  "availableHoursPerDay": 8
}
```

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "firstName": "Carlos",
  "lastName": "García",
  "email": "carlos.garcia@dsb.cl",
  "phoneNumber": "+56912345678",
  "workshopId": "550e8400-e29b-41d4-a716-446655440002",
  "assignedBrand": "Suzuki",
  "availableHoursPerDay": 8,
  "isActive": true,
  "createdAt": "2026-05-29T20:25:00Z"
}
```

#### Get Advisors

**Request:**
```http
GET /api/admin/advisors?pageNumber=1&pageSize=10
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "firstName": "Carlos",
      "lastName": "García",
      "email": "carlos.garcia@dsb.cl",
      "assignedBrand": "Suzuki",
      "workshopId": "550e8400-e29b-41d4-a716-446655440002"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

#### Update Advisor

**Request:**
```http
PUT /api/admin/advisors/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "availableHoursPerDay": 7,
  "phoneNumber": "+56987654321"
}
```

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "firstName": "Carlos",
  "lastName": "García",
  "email": "carlos.garcia@dsb.cl",
  "phoneNumber": "+56987654321",
  "availableHoursPerDay": 7,
  "assignedBrand": "Suzuki"
}
```

#### Delete Advisor

**Request:**
```http
DELETE /api/admin/advisors/{id}
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "message": "Advisor deleted successfully"
}
```

---

### 4. Schedule Management

#### Update Workshop Schedule (Weekly)

**Request:**
```http
PUT /api/admin/workshops/{workshopId}/schedule/Monday
Authorization: Bearer <token>
Content-Type: application/json

{
  "openTime": "09:00",
  "closeTime": "18:00"
}
```

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440004",
  "workshopId": "550e8400-e29b-41d4-a716-446655440002",
  "dayOfWeek": 1,
  "dayName": "Monday",
  "openTime": "09:00",
  "closeTime": "18:00",
  "isClosed": false
}
```

#### Get Workshop Schedule (Weekly)

**Request:**
```http
GET /api/admin/workshops/{workshopId}/schedule
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "dayOfWeek": 0,
    "dayName": "Sunday",
    "isClosed": true
  },
  {
    "dayOfWeek": 1,
    "dayName": "Monday",
    "openTime": "09:00",
    "closeTime": "18:00",
    "isClosed": false
  },
  {
    "dayOfWeek": 2,
    "dayName": "Tuesday",
    "openTime": "09:00",
    "closeTime": "18:00",
    "isClosed": false
  },
  {
    "dayOfWeek": 3,
    "dayName": "Wednesday",
    "openTime": "09:00",
    "closeTime": "18:00",
    "isClosed": false
  },
  {
    "dayOfWeek": 4,
    "dayName": "Thursday",
    "openTime": "09:00",
    "closeTime": "18:00",
    "isClosed": false
  },
  {
    "dayOfWeek": 5,
    "dayName": "Friday",
    "openTime": "09:00",
    "closeTime": "18:00",
    "isClosed": false
  },
  {
    "dayOfWeek": 6,
    "dayName": "Saturday",
    "openTime": "10:00",
    "closeTime": "16:00",
    "isClosed": false
  }
]
```

#### Create Holiday

**Request:**
```http
POST /api/admin/workshops/{workshopId}/holidays
Authorization: Bearer <token>
Content-Type: application/json

{
  "date": "2026-09-19",
  "reason": "Fiestas Patrias"
}
```

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440005",
  "workshopId": "550e8400-e29b-41d4-a716-446655440002",
  "date": "2026-09-19",
  "reason": "Fiestas Patrias",
  "createdAt": "2026-05-29T20:25:00Z"
}
```

#### Get Holidays

**Request:**
```http
GET /api/admin/workshops/{workshopId}/holidays?fromDate=2026-01-01&toDate=2026-12-31
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440005",
    "date": "2026-09-19",
    "reason": "Fiestas Patrias"
  }
]
```

#### Delete Holiday

**Request:**
```http
DELETE /api/admin/workshops/{workshopId}/holidays/{holidayId}
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "message": "Holiday deleted successfully"
}
```

#### Create Blackout Date (Closure Period)

**Request:**
```http
POST /api/admin/workshops/{workshopId}/blackouts
Authorization: Bearer <token>
Content-Type: application/json

{
  "startDate": "2026-06-15",
  "endDate": "2026-06-30",
  "reason": "Mantenimiento"
}
```

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440006",
  "workshopId": "550e8400-e29b-41d4-a716-446655440002",
  "startDate": "2026-06-15",
  "endDate": "2026-06-30",
  "reason": "Mantenimiento",
  "createdAt": "2026-05-29T20:25:00Z"
}
```

#### Get Blackout Dates

**Request:**
```http
GET /api/admin/workshops/{workshopId}/blackouts?fromDate=2026-01-01&toDate=2026-12-31
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440006",
    "startDate": "2026-06-15",
    "endDate": "2026-06-30",
    "reason": "Mantenimiento"
  }
]
```

#### Delete Blackout Date

**Request:**
```http
DELETE /api/admin/workshops/{workshopId}/blackouts/{blackoutDateId}
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "message": "Blackout date deleted successfully"
}
```

---

## Request/Response Examples

### Common Error Responses

#### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "Email address must be valid"
    ],
    "Password": [
      "Password must contain at least 8 characters"
    ]
  }
}
```

#### 401 Unauthorized
```json
{
  "error": "Invalid email or password"
}
```

#### 403 Forbidden
```json
{
  "error": "Access denied. Insufficient permissions."
}
```

#### 404 Not Found
```json
{
  "error": "Workshop not found"
}
```

#### 500 Internal Server Error
```json
{
  "error": "An unexpected error occurred"
}
```

---

## Error Handling

### Status Codes

| Code | Meaning | Common Causes |
|------|---------|---------------|
| 200 | OK | Successful request |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid input, validation failed |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 500 | Server Error | Unexpected server error |

### Validation Rules

#### Email
- Must be valid email format
- Must be unique across system
- Example: `user@dsb.cl`

#### Password
- Minimum 8 characters
- Must contain uppercase, lowercase, and number
- Optional special characters recommended
- Example: `SecurePass123!`

#### Workshop Name
- Cannot be empty
- Maximum 200 characters

#### Date Fields
- Must be ISO 8601 format (YYYY-MM-DD)
- Holiday dates must be today or later
- Blackout start date must be today or later
- Blackout end date must be >= start date

---

## Testing

### Unit Tests

```bash
# Run all tests
dotnet test Login.Api.Tests

# Run specific category
dotnet test Login.Api.Tests --filter "Category=Authentication"

# Generate coverage report
dotnet test Login.Api.Tests /p:CollectCoverage=true
```

### Test Coverage

| Category | Tests | Status |
|----------|-------|--------|
| Authentication | 8 | ✅ |
| User Management | 22 | ✅ |
| Workshop Management | 23 | ✅ |
| Advisor Management | 20 | ✅ |
| Schedule Management | 74 | ✅ |
| Integration Tests | 28 | ✅ |
| **TOTAL** | **175** | **✅** |

### Example Test

```csharp
[Fact]
public async Task CreateUser_WithValidData_ReturnsCreatedUser()
{
    // Arrange
    var command = new CreateUserCommand
    {
        Email = "test@dsb.cl",
        Password = "Test123!",
        FirstName = "Test",
        LastName = "User",
        RequestingUserId = Guid.NewGuid(),
        RequestingUserRoles = new[] { "SuperAdmin" }
    };

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Email.Should().Be("test@dsb.cl");
    result.FirstName.Should().Be("Test");
}
```

---

## References

- [ASP.NET Core Minimal APIs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [JWT.io](https://jwt.io/)
- [OpenAPI/Swagger](https://swagger.io/)
