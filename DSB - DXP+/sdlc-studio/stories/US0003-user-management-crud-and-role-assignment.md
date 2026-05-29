# US0003: User Management CRUD and Role Assignment

> **Status:** Draft
> **Epic:** [EP0002: Management API Core Entities](../epics/EP0002-management-api-core-entities.md)
> **Owner:** TBD
> **Reviewer:** TBD
> **Created:** 2026-05-29

## User Story

**As a** Super Administrator  
**I want** create, update, and manage users with role assignment  
**So that** platform access is governed consistently

## Context

### Persona Reference
**Super Administrator** - full platform governance  
[Full persona details](../personas.md#super-administrator)

## Acceptance Criteria

### AC1: User CRUD endpoints are available and validated
- **Given** valid user payloads
- **When** CRUD operations are executed
- **Then** users are created, updated, retrieved, and deactivated correctly

### AC2: Role assignment is enforced and auditable
- **Given** role assignment requests
- **When** roles are assigned/changed
- **Then** only authorized roles are accepted and persisted with traceability

### AC3: Scope boundaries are respected
- **Given** a Distributor Administrator
- **When** they attempt out-of-scope access
- **Then** access is denied with 403 Forbidden

