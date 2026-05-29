# PL0005: Advisor Management CRUD & Availability Configuration - Implementation Plan

> **Status:** Draft
> **Story:** [US0005: Advisor Management CRUD and Availability Configuration](../stories/US0005-advisor-management-crud-and-availability-configuration.md)
> **Epic:** [EP0002: Management API Core Entities](../epics/EP0002-management-api-core-entities.md)
> **Created:** 2026-05-29
> **Language:** C# (.NET 10)
> **Approach:** TDD (Test-First)

## Overview

Implement Advisor Management CRUD (Create, Read, Update, Delete) operations and advisor availability configuration. Advisors are workshop personnel who conduct service appointments.

## Acceptance Criteria Summary

| AC | Name | Description |
|----|------|-------------|
| AC1 | Advisor CRUD endpoints available | Create, read, update, deactivate advisors with validation |
| AC2 | Availability configured & validated | Working hours, specializations, schedule configurable |
| AC3 | Scope boundaries respected | Advisors scoped to assigned workshops only |

---

## Technical Context

### Domain Model
```
Advisor
├─ Id (Guid)
├─ FirstName (string, required)
├─ LastName (string, required)
├─ Email (string, required, unique)
├─ Phone (string)
├─ Specializations (collection: enum list)
├─ WorkshopId (Guid, FK)
├─ IsActive (bool)
├─ AvailabilitySchedule (complex type)
├─ CreatedAt, UpdatedAt (audit)
└─ Workshop (navigation)

AvailabilitySchedule
├─ Monday.StartTime (TimeSpan)
├─ Monday.EndTime (TimeSpan)
├─ ... (Tue-Sun)
├─ Vacation dates (collection)
└─ UnavailableDates (collection)
```

### API Endpoints (To Be Implemented)

#### Admin Endpoints (SuperAdminOnly)
- `POST /api/admin/advisors` - Create advisor
- `GET /api/admin/advisors` - List all advisors (paginated)
- `GET /api/admin/advisors/{id}` - Get advisor details
- `PUT /api/admin/advisors/{id}` - Update advisor
- `DELETE /api/admin/advisors/{id}` - Deactivate advisor
- `PUT /api/admin/advisors/{id}/availability` - Configure availability
- `POST /api/admin/advisors/{id}/vacation` - Add vacation period
- `DELETE /api/admin/advisors/{id}/vacation/{date}` - Remove vacation

#### Management Endpoints (AdminOrHigher - scoped by brand/workshop)
- `GET /api/management/advisors` - List advisors for assigned workshops
- `GET /api/management/advisors/{id}/availability` - Get advisor availability
- `GET /api/management/advisors/available` - Find available advisors for date/time

---

## Implementation Tasks

| # | Task | Effort | Status |
|---|------|--------|--------|
| 1 | Advisor entity & DbSet | 1h | [ ] |
| 2 | Write CRUD command tests | 4h | [ ] |
| 3 | Implement CRUD commands | 4h | [ ] |
| 4 | Write availability tests | 2h | [ ] |
| 5 | Implement availability config | 2h | [ ] |
| 6 | Create advisor endpoints | 2h | [ ] |
| 7 | Integration tests | 3h | [ ] |
| 8 | Documentation | 1h | [ ] |

**Total Effort:** ~19 hours

---

## Key Features

### Advisor Creation
- Validate advisor name and email
- Assign to workshop (validate workshop exists)
- Validate specializations exist
- Initialize availability schedule

### Availability Configuration
- Define working hours per day (Mon-Sun, typically same as workshop)
- Mark vacation dates
- Mark unavailable dates (sick leave, training)
- Validate no conflicts with workshop schedule
- Calculate available slots for booking

### Find Available Advisors
- Query advisors available for date/time
- Filter by specialization (if needed)
- Return sorted by seniority/rating

### Scope Enforcement
- DistributorAdmin sees only advisors in assigned brand workshops
- SuperAdmin sees all advisors
- Cross-workshop advisor assignment not allowed

---

## Related Files

- Database schema: Entity definition
- Validators: Business rule validation
- Queries/Commands: MediatR handlers
- Endpoints: REST API mapping
- Tests: Unit & integration tests

---

## Dependencies

- **Depends on:** US0003 (User Management), US0004 (Workshop Management)
- **Blocks:** Booking system implementation
- **Related:** Workshop availability aggregation

---

**Note:** This is the final story in Phase 1. After completion, the booking system (Phase 2) can begin.
