# US0002: Role-Based Access Control

> **Status:** ✅ Completed
> **Epic:** [EP0001: Authentication and RBAC Foundation](../epics/EP0001-auth-and-rbac-foundation.md)
> **Owner:** Development Team
> **Reviewer:** QA Team
> **Created:** 2026-05-29
> **Completed:** 2026-05-29

## User Story

**As a** Super Administrator  
**I want** role policies enforced on protected endpoints  
**So that** each user can access only authorized resources

## Context

### Persona Reference
**Distributor Administrator** - scoped operations by brand/region  
[Full persona details](../personas.md#distributor-administrator)

## Acceptance Criteria

### AC1: Role claims are enforced by middleware
- **Given** an authenticated user token with role claims
- **When** a protected endpoint is called
- **Then** authorization succeeds only if role policy allows it

### AC2: Unauthorized role access is blocked
- **Given** a user without required role/scope
- **When** they call a restricted endpoint
- **Then** the API returns 403 Forbidden

### AC3: Workshop users remain read-only where defined
- **Given** a Workshop User
- **When** they attempt write operations on restricted resources
- **Then** write operations are denied and read paths remain available

