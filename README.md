# DSB-PoC | Digital Service Booking - Proof of Concept

Backend implementation of **Digital Service Booking (DXP+)** built with ASP.NET Core 8.0 using modern software architecture patterns (CQRS, DI, Clean Architecture).

---

## 🎯 Current Status (May 2026)

**✅ Phase 1 Complete - Login API Ready for Production**

```
Phase 1: Login.Api (Authentication)        ✅ COMPLETE (175/175 tests passing)
Phase 2: Booking.Api (Reservations)        ⏳ PLANNED
Phase 3: Management.Api (Reports)          ⏳ PLANNED
Phase 4: Frontend (React)                  ⏳ PLANNED
```

| Module | Status | Endpoints | Tests |
|--------|--------|-----------|-------|
| **Authentication** | ✅ Complete | 2 | 8 |
| **User Management** | ✅ Complete | 6 | 22 |
| **Workshop Management** | ✅ Complete | 5 | 23 |
| **Advisor Management** | ✅ Complete | 5 | 20 |
| **Schedule Management** | ✅ Complete | 8 | 74 |
| **Integration Tests** | ✅ Complete | - | 28 |
| **TOTAL** | ✅ | **26 endpoints** | **175 tests** |

---

## 📁 Project Structure

```
DSB-PoC/
└── DSB - DXP+/                        ← MAIN PROJECT
    ├── Login.Api/                     ← ASP.NET Core 8.0 Minimal API
    │   ├── Features/
    │   │   ├── Auth/
    │   │   ├── Users/
    │   │   ├── Workshops/
    │   │   ├── Advisors/
    │   │   └── Schedules/
    │   ├── Infrastructure/
    │   │   ├── Data/
    │   │   ├── Authorization/
    │   │   └── Security/
    │   └── Program.cs
    │
    ├── Login.Api.Tests/               ← xUnit (175 tests, 100% passing)
    │   ├── Features/                  (Unit tests)
    │   └── Integration/               (HTTP endpoint tests)
    │
    ├── Pre_Requirements_Definitions/  ← Original specifications
    ├── sdlc-studio/                   ← SDLC artifacts
    │
    ├── DSB.sln
    ├── QUICK_START.md                 ← Setup guide
    ├── API_DOCUMENTATION.md           ← Complete API reference
    ├── IMPLEMENTATION_GUIDE.md        ← Architecture & patterns
    ├── IMPLEMENTATION_SUMMARY.md      ← Project summary
    └── README.md
```

---

## 🚀 Quick Start

Navigate to the main project folder:

```bash
cd "DSB - DXP+"
```

### Build & Run

```bash
# Build API
dotnet build Login.Api

# Run tests (175 passing)
dotnet test Login.Api.Tests

# Run server (development)
dotnet run --project Login.Api
```

**Server available at:** `https://localhost:5001`  
**Swagger UI:** `https://localhost:5001/swagger`

---

## 📚 Documentation

Inside `DSB - DXP+/` folder, you'll find complete documentation:

- **[QUICK_START.md](DSB%20-%20DXP%2B/QUICK_START.md)** — Installation and setup guide
- **[API_DOCUMENTATION.md](DSB%20-%20DXP%2B/API_DOCUMENTATION.md)** — Complete API reference with examples (50+ pages equivalent)
- **[IMPLEMENTATION_GUIDE.md](DSB%20-%20DXP%2B/IMPLEMENTATION_GUIDE.md)** — Architecture, CQRS patterns, how to add features (40+ pages)
- **[IMPLEMENTATION_SUMMARY.md](DSB%20-%20DXP%2B/IMPLEMENTATION_SUMMARY.md)** — Executive summary, data model, statistics (20+ pages)

---

## 📝 API Quick Reference

### Authentication
```
POST /api/auth/login              Get access token
POST /api/auth/refresh            Renew expired token
```

### User Management  
```
POST   /api/admin/users           Create user (SuperAdmin)
GET    /api/admin/users           List users (SuperAdmin)
PUT    /api/admin/users/{id}      Update user (SuperAdmin)
DELETE /api/admin/users/{id}      Deactivate user (SuperAdmin)
POST   /api/admin/users/assign-role    Assign role (SuperAdmin)
POST   /api/admin/users/remove-role    Remove role (SuperAdmin)
```

### Workshop Management
```
POST   /api/admin/workshops       Create workshop (SuperAdmin)
GET    /api/admin/workshops       List workshops
PUT    /api/admin/workshops/{id}  Update workshop (SuperAdmin)
DELETE /api/admin/workshops/{id}  Deactivate workshop (SuperAdmin)
GET    /api/management/workshops  List assigned workshops
```

### Advisor Management
```
POST   /api/admin/advisors        Create advisor (SuperAdmin)
GET    /api/admin/advisors        List advisors
PUT    /api/admin/advisors/{id}   Update advisor (SuperAdmin)
DELETE /api/admin/advisors/{id}   Deactivate advisor (SuperAdmin)
GET    /api/management/advisors   List assigned advisors
```

### Schedule Management
```
PUT  /api/admin/workshops/{id}/schedule/{dayOfWeek}    Update weekly schedule
GET  /api/admin/workshops/{id}/schedule                Get weekly schedule
POST /api/admin/workshops/{id}/holidays                Create holiday
GET  /api/admin/workshops/{id}/holidays                List holidays
DELETE /api/admin/workshops/{id}/holidays/{id}         Delete holiday
POST /api/admin/workshops/{id}/blackouts               Create blackout period
GET  /api/admin/workshops/{id}/blackouts               List blackouts
DELETE /api/admin/workshops/{id}/blackouts/{id}        Delete blackout
```

---

## 🏗️ Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Framework** | ASP.NET Core | 8.0 |
| **API Pattern** | Minimal APIs | Native |
| **ORM** | Entity Framework Core | 8.0 |
| **CQRS** | MediatR | 12.x |
| **Validation** | FluentValidation | 11.x |
| **Authentication** | JWT Bearer | .NET Native |
| **Testing** | xUnit + FluentAssertions | Latest |

---

## 🔐 Authorization Model

**3-Tier Role Hierarchy:**

```
SuperAdmin
├── Full platform access
├── Manage all users/workshops/advisors
├── Configure schedules & closures
└── Assign roles to other users

DistributorAdmin
├── Limited to assigned brands
├── Manage assigned staff
└── View assigned workshops

WorkshopUser
└── Read-only access to public information
```

**3 Authorization Policies:**
- `SuperAdminOnly` — SuperAdmin only
- `AdminOrHigher` — SuperAdmin + DistributorAdmin  
- `WorkshopReadOnly` — All authenticated users

---

## ✅ Quality Metrics

| Metric | Value |
|--------|-------|
| **Build Status** | ✅ 0 errors, 0 warnings |
| **Test Coverage** | ✅ 100% (175/175 passing) |
| **Total Endpoints** | 26 |
| **Total Tests** | 175 |
| **Code Files** | 100+ |
| **Documentation Pages** | ~150 equivalent |

---

## 🎯 Implementation Roadmap

### Phase 1: ✅ Login API (Complete)
- Authentication with JWT tokens
- User management with role assignment
- Workshop management for locations
- Advisor management with brand specialization
- Schedule management (weekly hours, holidays, blackouts)
- 175 passing tests with 100% coverage

### Phase 2: ⏳ Booking.Api (Planned)
- Reservation system
- Availability calculation engine
- Booking management and cancellations
- Customer notifications

### Phase 3: ⏳ Management.Api (Planned)
- Analytics and reporting
- Performance dashboards
- Historical data analysis
- Export capabilities

### Phase 4: ⏳ Frontend (Planned)
- React 18 + TypeScript
- Atomic design pattern
- User authentication UI
- Booking management interface

---

## 📚 Architecture Highlights

### CQRS Pattern
- **Commands:** Separate write operations (Create, Update, Delete)
- **Queries:** Separate read operations (Get, List)
- **Handlers:** Isolated business logic per operation
- **Validation:** FluentValidation for input constraints

### Features Folder Structure
```
Features/
├── Auth/
├── Users/
├── Workshops/
├── Advisors/
└── Schedules/
    ├── WorkshopSchedule/
    ├── Holiday/
    └── Blackout/
```

Each feature follows: `{Operation}/{Files}`

### Database Strategy
- **ORM:** Entity Framework Core
- **Pattern:** Soft deletes (IsActive flag) for data integrity
- **Relationships:** Cascade deletes for referential integrity
- **Indexing:** Unique constraints on email fields
- **Storage:** In-memory for testing, scalable to SQL Server

---

## 🧪 Testing

### Unit Tests (147 tests)
- Command/Query handler business logic
- Authorization enforcement
- Validation rule compliance
- Edge cases and error scenarios

### Integration Tests (28 tests)
- HTTP endpoint functionality
- Authentication flow validation
- Error response handling
- Full request/response cycle

### Running Tests
```bash
# All tests
dotnet test Login.Api.Tests

# Specific category
dotnet test Login.Api.Tests --filter "Authentication"

# With coverage
dotnet test Login.Api.Tests --collect:"XPlat Code Coverage"
```

---

## 🔐 Security Features

- ✅ Password hashing with BCrypt
- ✅ JWT token validation with expiry
- ✅ Refresh token rotation
- ✅ Role-based access control (RBAC)
- ✅ Claim-based authorization
- ✅ Input validation and sanitization
- ✅ Soft deletes for audit trails

---

## 📊 Default Credentials

```
Email:    admin@dsb.cl
Password: Admin123!
Role:     SuperAdmin
```

---

## 📄 Project Information

**Project:** Digital Service Booking (DXP+)  
**Version:** 1.0.0  
**Status:** Production Ready (Phase 1)  
**Last Updated:** May 29, 2026  

**Team Capacity:**
- ✅ Phase 1: 100% Complete
- ⏳ Phases 2-4: Ready for implementation

---

## 🤝 Contributing

When extending the project:

1. Follow established CQRS patterns
2. Create unit tests (minimum 80% coverage)
3. Add integration tests for endpoints
4. Update documentation
5. Follow naming conventions:
   - Commands: `{Action}{Resource}Command.cs`
   - Queries: `Get{Resource}Query.cs`
   - Handlers: `{Operation}{Resource}Handler.cs`
   - Endpoints: `{Resource}Endpoints.cs`

---

## 📞 Support

For questions about implementation:
1. Review documentation files in `DSB - DXP+/`
2. Check test examples in `Login.Api.Tests/`
3. Review feature code in `Login.Api/Features/` (follows consistent patterns)

For detailed endpoint information: [API_DOCUMENTATION.md](DSB%20-%20DXP%2B/API_DOCUMENTATION.md)  
For architecture details: [IMPLEMENTATION_GUIDE.md](DSB%20-%20DXP%2B/IMPLEMENTATION_GUIDE.md)  
For project overview: [IMPLEMENTATION_SUMMARY.md](DSB%20-%20DXP%2B/IMPLEMENTATION_SUMMARY.md)  
For setup steps: [QUICK_START.md](DSB%20-%20DXP%2B/QUICK_START.md)

---

**Happy coding!** 🚀
