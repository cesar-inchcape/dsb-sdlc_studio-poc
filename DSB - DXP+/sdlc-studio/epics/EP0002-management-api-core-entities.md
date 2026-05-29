# EP0002: Management API Core Entities

> **Status:** 🔄 Planned (Stories decomposed)
> **Owner:** Development Team
> **Reviewer:** TBD
> **Created:** 2026-05-29
> **Target Release:** Phase 1 (MVP)
> **Progress:** 3/3 stories have implementation plans ✅

## Summary

Deliver CRUD and operational controls for users, workshops, and advisors in Management.Api with role-aware behavior.

## Business Context

### Problem Statement
Phase 1 requires complete management capabilities for core entities while respecting role scopes and governance policies.

**PRD Reference:** [FR-USER-001..004, FR-WORKSHOP-001..002, FR-ADVISOR-001..002](../prd.md#42-user-management-managementapi)

### Value Proposition
- Enables day-to-day operational management
- Improves data quality and governance
- Supports real booking readiness in later phases

## Scope

### In Scope
- User CRUD with role assignment
- Workshop CRUD and schedule configuration
- Advisor CRUD and availability setup
- Scope-based authorization checks

### Out of Scope
- Booking flow implementation
- Reporting dashboards
- DMS integration

### Affected Personas
- **Super Administrator:** full cross-brand configuration control
- **Distributor Administrator:** operational control within assigned scope
- **Workshop User:** read-only visibility for service configuration

## Acceptance Criteria (Epic Level)

- [ ] User, workshop, and advisor CRUD endpoints are available and validated
- [ ] Role and scope restrictions are enforced on all management endpoints
- [ ] Scheduling and blocking inputs are reflected in persisted data models

## Story Breakdown

- [ ] [US0003: User Management CRUD and Role Assignment](../stories/US0003-user-management-crud-and-role-assignment.md)
- [ ] [US0004: Workshop Management CRUD and Schedule Configuration](../stories/US0004-workshop-management-crud-and-schedule-configuration.md)
- [ ] [US0005: Advisor Management CRUD and Availability Configuration](../stories/US0005-advisor-management-crud-and-availability-configuration.md)

