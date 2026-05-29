# Product Requirements Document

**Project:** Digital Service Booking (DSB) - DXP+  
**Version:** 1.0.0  
**Last Updated:** 2026-05-28  
**Status:** Draft  

---

## 1. Project Overview

###

 Product Name
**Digital Service Booking (DSB)** — Third-Party Dealer Platform for Chile

### Purpose
Provide a comprehensive digital service booking platform for third-party automotive dealers (Americar and Autum) operating multiple brands (Suzuki, Changan, Mazda, Renault, GWM, Avatr, Deepal, DSFK) in Chile. The platform enables customers to book service appointments online while providing dealers with workshop management, advisor scheduling, and operational reporting capabilities.

### Tech Stack

**Backend:**
- .NET 10 (ASP.NET Core Web API)
- Entity Framework Core + SQL Server
- Vertical Slice Architecture
- MediatR (CQRS pattern)
- FluentValidation

**Frontend:**
- React 18 + TypeScript
- Vite (Build tool)
- Atomic Design Pattern
- Axios (HTTP client)

### Architecture Pattern
**Microservices** with Vertical Slice Architecture within each service:
- **Login.Api** — Authentication & Authorization
- **Booking.Api** — Scheduling, Availability, Reporting
- **Management.Api** — Workshops, Advisors, Users, Client Database

---

## 2. Problem Statement

### Problem Being Solved
Third-party automotive dealers in Chile lack a modern, integrated digital booking system that can:
1. Handle multiple brands under different dealer groups
2. Integrate with existing Dealer Management Systems (DMS)
3. Provide role-based access control for different user types (admins, dealers, workshop staff)
4. Support both manual and integrated workflows for non-technical dealers

### Target Users
1. **End Customers** — Vehicle owners booking service appointments
2. **Workshop Staff** — Read-only access to service configurations
3. **Retail Group Users** — Multi-workshop access with configurable permissions
4. **Distributor Administrators** — Manage operations within assigned scope (brand/region)
5. **Super Administrators** — Full platform control across all dealers and brands

### Context
- **Dealers:** Americar, Autum (3PD - Third-Party Dealers)
- **Brands:** Suzuki, Changan, Mazda, Renault, GWM, Avatr, Deepal, DSFK
- **Market:** Chile
- **Integration:** Must integrate with existing Dealer Management Systems (DMS)
- **Workflow:** Backend must be implemented first (Frontend depends on active APIs)

---

## 3. Feature Inventory

| Feature | Description | Status | Priority | Location |
|---------|-------------|--------|----------|----------|
| Authentication & Authorization | JWT-based auth with RBAC | Not Started | Must-Have | Login.Api |
| User Management | Create, edit, delete users with role assignment | Not Started | Must-Have | Management.Api |
| Advisor Management | CRUD operations for service advisors | Not Started | Must-Have | Management.Api |
| Workshop Management | Configure workshops, schedules, and blocks | Not Started | Must-Have | Management.Api |
| Service Booking | Customer-facing appointment scheduling | Not Started | Must-Have | Booking.Api |
| Availability Management | Real-time slot calculation and blocking | Not Started | Must-Have | Booking.Api |
| Reporting & Analytics | Operational reports and dashboards | Not Started | Should-Have | Booking.Api |
| Client Database | Customers, vehicles, brands, geography | Not Started | Must-Have | Management.Api |

---

## 4. Functional Requirements

### 4.1 Authentication & Authorization (Login.Api)

#### FR-AUTH-001: Secure JWT Authentication
**User Story:** As a user, I want to log in securely so that I can access the system based on my role.

**Acceptance Criteria:**
- [ ] System validates username/email and hashed password
- [ ] System generates JWT token with role claims
- [ ] Token expires after configurable time period
- [ ] System supports token refresh mechanism

**Dependencies:** None  
**Status:** Not Started  
**Confidence:** [HIGH]

#### FR-AUTH-002: Role-Based Access Control (RBAC)
**User Story:** As a system administrator, I want to assign roles to users so that they have appropriate permissions.

**Acceptance Criteria:**
- [ ] System supports 4 role types: Super Admin, Distributor Admin, Retail Group User, Workshop User
- [ ] Each role has configurable permissions
- [ ] Middleware enforces role-based access on all protected endpoints
- [ ] Unauthorized access returns HTTP 403 Forbidden

**Dependencies:** FR-AUTH-001  
**Status:** Not Started  
**Confidence:** [HIGH]

---

### 4.2 User Management (Management.Api)

#### FR-USER-001: Super Administrator Full Platform Control
**User Story:** As a super administrator, I want to manage all profiles, business rules, and workshop configurations across all brands so that I can centrally govern the entire Service Booking platform.

**Acceptance Criteria:**
- [ ] **AC1 – Full Configuration Permissions:** Super admin can create, edit, and delete: Users, Roles, Permissions, Business Rules, Workshops, Services, Integrations, Retail Groups, Dealers
- [ ] **AC2 – Cross-Brand Visibility:** Super admin can select and view any dealer, brand, region, workshop, or group
- [ ] **AC3 – Unrestricted Access:** No feature, rule, or configuration is restricted by scope

**Dependencies:** FR-AUTH-002  
**Status:** Not Started  
**Confidence:** [HIGH]  
**Source:** DXPOWN-3812.md

#### FR-USER-002: Distributor Administrator Restricted to Own Scope
**User Story:** As a distributor administrator, I want to manage only my assigned scope so that I can operate within my brand/region boundaries.

**Acceptance Criteria:**
- [ ] Distributor admin can only access assigned distributor/brand/region
- [ ] System filters all data queries by distributor scope
- [ ] Attempts to access out-of-scope data return HTTP 403 Forbidden

**Dependencies:** FR-AUTH-002  
**Status:** Not Started  
**Confidence:** [HIGH]  
**Source:** DXPOWN-3813.md

#### FR-USER-003: Workshop User With Read-Only Service Permissions
**User Story:** As a workshop user, I want view-only access to service configurations so that I can reference but not modify them.

**Acceptance Criteria:**
- [ ] Workshop user can view service configurations
- [ ] All edit/delete actions return HTTP 403 Forbidden
- [ ] User interface hides edit/delete buttons for workshop users

**Dependencies:** FR-AUTH-002  
**Status:** Not Started  
**Confidence:** [HIGH]  
**Source:** DXPOWN-3814.md

#### FR-USER-004: Retail Group User With Multi-Workshop Access
**User Story:** As a retail group user, I want access to multiple workshops with permissions tailored to my profile so that I can operate only within the areas assigned to me.

**Acceptance Criteria:**
- [ ] **AC1 – Multi-Workshop Assignment:** User can access multiple workshops belonging to the retail group
- [ ] **AC2 – Custom Function Visibility:** System enables or hides features based on assigned permissions (Create, Edit, View, Delete)
- [ ] **AC3 – Admin Panel for Permissions:** System admin can toggle visibility of: Reporting, Workshop management, Service management, Appointment operations

**Dependencies:** FR-AUTH-002  
**Status:** Not Started  
**Confidence:** [HIGH]  
**Source:** DXPOWN-3815.md

---

### 4.3 Advisor Management (Management.Api)

#### FR-ADVISOR-001: CRUD Operations for Advisors
**User Story:** As a distributor administrator, I want to create, update, and delete advisor profiles so that I can manage my service team.

**Acceptance Criteria:**
- [ ] System supports Create, Read, Update, Delete for advisor profiles
- [ ] Advisor profile includes: Name, Email, Phone, Workshop Assignment, Active/Inactive status
- [ ] System validates required fields before saving
- [ ] Deleting an advisor with active bookings shows warning and requires confirmation

**Dependencies:** FR-USER-001  
**Status:** Not Started  
**Confidence:** [HIGH]

#### FR-ADVISOR-002: Advisor Schedule Management
**User Story:** As a distributor administrator, I want to configure advisor schedules and availability blocks so that customers can book only when advisors are available.

**Acceptance Criteria:**
- [ ] System allows defining weekly schedules (day, start time, end time) per advisor
- [ ] System supports ad-hoc availability blocks (vacation, sick leave, training)
- [ ] Blocked time periods exclude advisor from availability calculations
- [ ] Changes to schedule update availability in real-time

**Dependencies:** FR-ADVISOR-001  
**Status:** Not Started  
**Confidence:** [MEDIUM]

---

### 4.4 Workshop Management (Management.Api)

#### FR-WORKSHOP-001: Workshop Configuration
**User Story:** As a super administrator, I want to create and configure workshops so that I can manage dealer locations.

**Acceptance Criteria:**
- [ ] System supports Create, Read, Update, Delete for workshops
- [ ] Workshop profile includes: Name, Address, City, Region, Brand(s), Operating Hours, Active/Inactive status
- [ ] System validates required fields before saving
- [ ] Deleting a workshop with active bookings shows warning and requires confirmation

**Dependencies:** FR-USER-001  
**Status:** Not Started  
**Confidence:** [HIGH]

#### FR-WORKSHOP-002: Workshop Schedule and Blocking
**User Story:** As a distributor administrator, I want to configure workshop schedules and availability blocks so that customers can book only during operating hours.

**Acceptance Criteria:**
- [ ] System allows defining weekly operating schedules (day, open time, close time) per workshop
- [ ] System supports ad-hoc workshop blocks (holidays, maintenance, events)
- [ ] Blocked time periods exclude workshop from availability calculations
- [ ] Changes to schedule update availability in real-time

**Dependencies:** FR-WORKSHOP-001  
**Status:** Not Started  
**Confidence:** [MEDIUM]

---

### 4.5 Service Booking (Booking.Api)

#### FR-BOOKING-001: Customer Appointment Scheduling
**User Story:** As a customer, I want to book a service appointment online so that I don't have to call the workshop.

**Acceptance Criteria:**
- [ ] Customer can select: Brand, Workshop, Service Type, Date, Time Slot, Advisor (optional)
- [ ] System displays only available time slots based on workshop/advisor schedules
- [ ] System confirms booking and sends confirmation (email/SMS)
- [ ] System stores booking with status: Scheduled

**Dependencies:** FR-WORKSHOP-001, FR-ADVISOR-001  
**Status:** Not Started  
**Confidence:** [HIGH]

#### FR-BOOKING-002: Availability Calculation
**User Story:** As a system, I want to calculate available time slots in real-time so that customers only see bookable slots.

**Acceptance Criteria:**
- [ ] System calculates availability based on: Workshop schedules, Advisor schedules, Existing bookings, Availability blocks
- [ ] System uses SQL Server Stored Procedure for complex availability queries
- [ ] Availability updates in real-time when schedules or bookings change
- [ ] System handles concurrent booking attempts (race conditions)

**Dependencies:** FR-BOOKING-001  
**Status:** Not Started  
**Confidence:** [MEDIUM]

---

### 4.6 Reporting & Analytics (Booking.Api)

#### FR-REPORT-001: Operational Dashboards
**User Story:** As a distributor administrator, I want to view real-time dashboards so that I can monitor workshop utilization and booking trends.

**Acceptance Criteria:**
- [ ] Dashboard displays: Total bookings (today/week/month), Workshop utilization rate, Advisor utilization rate, Booking status breakdown (Scheduled/Confirmed/Completed/Cancelled)
- [ ] Data refreshes automatically every 5 minutes
- [ ] User can filter by: Date range, Workshop, Brand, Service Type

**Dependencies:** FR-BOOKING-001  
**Status:** Not Started  
**Confidence:** [MEDIUM]

#### FR-REPORT-002: Exportable Reports
**User Story:** As a distributor administrator, I want to export booking data so that I can analyze it in external tools.

**Acceptance Criteria:**
- [ ] System supports export formats: CSV, Excel (XLSX), JSON
- [ ] Export includes all booking fields with filters applied
- [ ] System limits export to user's scope (based on role)
- [ ] Export generates asynchronously for large datasets

**Dependencies:** FR-REPORT-001  
**Status:** Not Started  
**Confidence:** [LOW]

---

## 5. Non-Functional Requirements

### 5.1 Performance
- **Response Time:** API endpoints must respond within 200ms for 95% of requests
- **Availability Calculation:** Complex availability queries (using Stored Procedures) must complete within 500ms
- **Concurrent Users:** System must support 500 concurrent users without degradation
- **Database:** Use database indexing on frequently queried columns (Workshop ID, Advisor ID, Booking Date)

**Confidence:** [MEDIUM]

### 5.2 Security
- **Authentication:** JWT tokens with HTTPS-only transmission
- **Password Storage:** Passwords must be hashed using bcrypt with salt
- **SQL Injection Prevention:** All database queries use parameterized calls (EF Core + Stored Procedures)
- **CORS:** Configure CORS to allow only whitelisted Frontend origins
- **Rate Limiting:** Implement rate limiting on authentication endpoints (5 attempts per minute)

**Confidence:** [HIGH]

### 5.3 Scalability
- **Horizontal Scaling:** APIs must be stateless to support horizontal scaling
- **Database Connection Pooling:** Use EF Core connection pooling (min: 5, max: 100)
- **Caching:** Future: Implement Redis caching for availability calculations
- **Message Queue:** Future: Implement message queue for async booking notifications

**Confidence:** [MEDIUM]

### 5.4 Availability
- **Uptime Target:** 99.5% uptime during business hours (8 AM - 8 PM Chile time)
- **Error Handling:** All endpoints return structured error responses with HTTP status codes
- **Logging:** Log all errors, warnings, and audit events to centralized logging system
- **Health Checks:** Implement /health endpoint for monitoring

**Confidence:** [HIGH]

---

## 6. Data Architecture

### 6.1 Data Models

**Core Entities:**
- **Users:** User profiles with role assignments
- **Roles:** Role definitions (Super Admin, Distributor Admin, etc.)
- **Permissions:** Granular permissions (Create, Read, Update, Delete per resource)
- **Workshops:** Workshop locations with schedules
- **Advisors:** Service advisor profiles with schedules
- **Bookings:** Customer appointments with status tracking
- **AvailabilitySlots:** Pre-calculated availability matrix
- **Customers:** Customer profiles
- **Vehicles:** Vehicle information
- **ServiceTypes:** Available service types per brand
- **Brands:** Automotive brands supported

### 6.2 Relationships and Constraints
- Users → Roles: Many-to-Many (UserRoles junction table)
- Roles → Permissions: Many-to-Many (RolePermissions junction table)
- Workshops → Brands: Many-to-Many (WorkshopBrand junction table)
- Workshops → Advisors: One-to-Many
- Bookings → Customers: Many-to-One
- Bookings → Workshops: Many-to-One
- Bookings → Advisors: Many-to-One
- Bookings → ServiceTypes: Many-to-One

### 6.3 Storage Mechanisms
- **Primary Database:** SQL Server (latest stable version)
- **EF Core Migrations:** For schema changes
- **Stored Procedures:** For complex queries (availability calculations, reports)
- **Data Types:**
  - IDs: uniqueidentifier (GUID)
  - Text: nvarchar(N) with defined length
  - Dates: datetime2
  - Booleans: bit
  - Decimals: decimal(18,2)

**Confidence:** [HIGH]

---

## 7. Integration Map

### 7.1 External Services
- **Dealer Management Systems (DMS):** Integration for customer/vehicle data sync (future)
- **Email/SMS Service:** For booking confirmations and reminders (future)
- **Payment Gateway:** For online payment processing (future phase)

### 7.2 Authentication Methods
- **JWT Tokens:** Issued by Login.Api, validated by all services
- **Password Hashing:** bcrypt with salt
- **Token Expiration:** 24 hours (configurable)
- **Refresh Tokens:** 7 days (configurable)

### 7.3 Third-Party Dependencies
- **.NET 10:** Runtime environment
- **Entity Framework Core:** ORM
- **MediatR:** CQRS pattern implementation
- **FluentValidation:** DTO validation
- **Serilog:** Logging (future)
- **React 18:** Frontend framework
- **Axios:** HTTP client
- **Vite:** Frontend build tool

**Confidence:** [HIGH]

---

## 8. Configuration Reference

### 8.1 Environment Variables

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `DATABASE_CONNECTION_STRING` | SQL Server connection string | Yes | - |
| `JWT_SECRET` | Secret key for JWT signing | Yes | - |
| `JWT_EXPIRATION_HOURS` | Token expiration time | No | 24 |
| `CORS_ALLOWED_ORIGINS` | Allowed Frontend origins | Yes | - |
| `API_BASE_URL` | Base URL for API | Yes | - |
| `ENVIRONMENT` | Deployment environment | No | Development |

### 8.2 Feature Flags
- **EnableDMSIntegration:** Enable/disable DMS sync (default: false)
- **EnableEmailNotifications:** Enable/disable email notifications (default: false)
- **EnableSMSNotifications:** Enable/disable SMS notifications (default: false)

**Confidence:** [MEDIUM]

---

## 9. Testing Requirements

### 9.1 Definition of Done
**No feature is production-ready without:**
1. **Unit Tests:** Coverage of business logic (Services/Use Cases)
2. **API Tests:** Validation of endpoints (status codes, payloads, schemas)
3. **Test Execution:** All tests must pass in CI/CD pipeline
4. **Code Review:** Peer review approval required

### 9.2 Testing Standards
- **Backend Unit Tests:** Mock database layer (EF Core DbContext)
- **Backend API Tests:** Test against real endpoints (integration tests)
- **Frontend Unit Tests:** Test components, hooks, and state management
- **Test Naming:** English language for consistency with architecture
- **Test Commands:**
  - Backend: `dotnet test`
  - Frontend: `npm run test`

### 9.3 Quality Gates
- **CI/CD:** Every Pull Request triggers full test suite
- **Merge Blocker:** Failing tests block merge
- **Coverage Target:** Minimum 70% code coverage (future goal)

**Confidence:** [HIGH]  
**Source:** testing_requirements.md

---

## 10. Implementation Priority

> **⚠️ IMPORTANT:** Only **Phase 1** is in scope for current development. Phase 2 and Phase 3 are deferred to future iterations.

### Phase 1 (MVP - Must-Have) — **CURRENT SCOPE**
1. **Login.Api** — Authentication & Authorization
2. **Management.Api** — Users, Roles, Workshops, Advisors (CRUD)

**Phase 1 Deliverables:**
- Complete user authentication with JWT
- Role-based access control (4 role types)
- Workshop management (CRUD + configuration)
- Advisor management (CRUD + scheduling configuration)
- User management (CRUD + role assignment)
- All Phase 1 features must include unit tests and API tests

### Phase 2 (Deferred - Future Implementation)
3. **Booking.Api** — Appointment scheduling, availability calculation, client database
4. **Reporting & Analytics** — Dashboards and exports
5. **Advanced Scheduling** — Availability blocks, complex schedules
6. **DMS Integration** — Customer/vehicle data sync

### Phase 3 (Future Enhancements)
7. **Email/SMS Notifications** — Automated reminders
8. **Payment Integration** — Online payment processing
9. **Mobile App** — Native mobile experience

---

## 11. Open Questions

1. **DMS Integration Timing:** When will DMS integration be required? (Affects Phase 2 timeline)
2. **Email/SMS Provider:** Which service provider should be used for notifications?
3. **Payment Gateway:** Which payment gateway is preferred for Chile market?
4. **Multi-language Support:** Will the system need to support multiple languages? (Currently designed for Spanish)
5. **Appointment Reminders:** What is the preferred timing for appointment reminders? (24h, 48h, 72h?)

---

## 12. Recommendations

1. **Phase 1 Focus:** Implement only Login.Api and Management.Api for current iteration. Booking.Api is deferred to Phase 2.
2. **Start with Backend:** Implement Login.Api first, then Management.Api (Frontend depends on APIs)
3. **Use Vertical Slice Architecture:** Each feature should be self-contained within its slice
4. **Prioritize Testing:** Set up test infrastructure early (xUnit for Backend, Jest/Vitest for Frontend)
5. **Database Indexing:** Add indexes early for Workshop ID, Advisor ID
6. **API Documentation:** Use Swagger/OpenAPI for API documentation
7. **Error Logging:** Implement structured logging early (Serilog recommended)

---

**Document Status:** Generated from Pre_Requirements_Definitions  
**Next Steps:** 
1. Review and validate this PRD
2. Run `/sdlc-studio epic` to generate Epics
3. Run `/sdlc-studio trd generate` to extract Technical Requirements Document
