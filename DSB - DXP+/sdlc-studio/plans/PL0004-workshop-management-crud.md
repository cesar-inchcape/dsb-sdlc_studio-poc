# PL0004: Workshop Management CRUD & Schedule Configuration - Implementation Plan

> **Status:** Draft
> **Story:** [US0004: Workshop Management CRUD and Schedule Configuration](../stories/US0004-workshop-management-crud-and-schedule-configuration.md)
> **Epic:** [EP0002: Management API Core Entities](../epics/EP0002-management-api-core-entities.md)
> **Created:** 2026-05-29
> **Language:** C# (.NET 10)
> **Approach:** TDD (Test-First)

## Overview

Implement Workshop Management CRUD (Create, Read, Update, Delete) operations and workshop schedule configuration. Workshops are the core scheduling units where services are delivered by advisors.

## Acceptance Criteria Summary

| AC | Name | Description |
|----|------|-------------|
| AC1 | Workshop CRUD endpoints available | Create, read, update, deactivate workshops with validation |
| AC2 | Schedule configuration managed | Operating hours, holidays, blackout dates configurable |
| AC3 | Multi-brand support validated | Workshop brand assignment and filtering works correctly |

---

## Technical Context

### Domain Model
```
Workshop
├─ Id (Guid)
├─ Name (string, required)
├─ Brand (enum: Suzuki, Changan, Mazda, Renault, GWM, Avatr, Deepal, DSFK)
├─ Location (string, required)
├─ Address (complex type)
├─ Capacity (int, advisory spots per day)
├─ OperatingHours (schedule)
├─ Holidays (collection)
├─ BlackoutDates (collection)
├─ IsActive (bool)
├─ CreatedAt, UpdatedAt (audit)
└─ Advisors (navigation - one-to-many)
```

### API Endpoints (To Be Implemented)

#### Admin Endpoints (SuperAdminOnly)
- `POST /api/admin/workshops` - Create workshop
- `GET /api/admin/workshops` - List all workshops (paginated)
- `GET /api/admin/workshops/{id}` - Get workshop details
- `PUT /api/admin/workshops/{id}` - Update workshop
- `DELETE /api/admin/workshops/{id}` - Deactivate workshop
- `PUT /api/admin/workshops/{id}/schedule` - Configure operating hours
- `POST /api/admin/workshops/{id}/holidays` - Add holiday
- `DELETE /api/admin/workshops/{id}/holidays/{date}` - Remove holiday

#### Management Endpoints (AdminOrHigher - scoped by brand)
- `GET /api/management/workshops` - List workshops for assigned brands
- `GET /api/management/workshops/{id}/schedule` - Get workshop schedule
- `POST /api/management/workshops/{id}/appointments` - Create appointment slot

---

## Implementation Tasks

| # | Task | Effort | Status |
|---|------|--------|--------|
| 1 | Workshop entity & DbSet | 1h | [ ] |
| 2 | Write CRUD command tests | 4h | [ ] |
| 3 | Implement CRUD commands | 4h | [ ] |
| 4 | Write schedule tests | 2h | [ ] |
| 5 | Implement schedule configuration | 2h | [ ] |
| 6 | Create workshop endpoints | 2h | [ ] |
| 7 | Integration tests | 3h | [ ] |
| 8 | Documentation | 1h | [ ] |

**Total Effort:** ~19 hours

---

## Key Features

### Workshop Creation
- Validate workshop name and brand
- Validate location is unique per brand
- Initialize default operating hours
- Create advisors collection

### Schedule Configuration
- Define operating hours (per day: Mon-Sun, hours 08:00-18:00)
- Mark holidays (e.g., Dec 25, New Year)
- Define blackout dates (maintenance, special events)
- Validate no overlapping blackout periods

### Brand Filtering
- DistributorAdmin can see only assigned brand workshops
- SuperAdmin can see all workshops
- Scope enforcement at query level

---

## Related Files

- Database schema: Entity definition
- Validators: Business rule validation
- Queries/Commands: MediatR handlers
- Endpoints: REST API mapping
- Tests: Unit & integration tests

---

**Note:** This story depends on US0003 (User Management) for advisor assignment.
