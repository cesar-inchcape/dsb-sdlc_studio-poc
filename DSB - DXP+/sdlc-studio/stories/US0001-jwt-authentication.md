# US0001: JWT Authentication

> **Status:** ✅ Completed
> **Epic:** [EP0001: Authentication and RBAC Foundation](../epics/EP0001-auth-and-rbac-foundation.md)
> **Owner:** Development Team
> **Reviewer:** QA Team
> **Created:** 2026-05-29
> **Completed:** 2026-05-29

## User Story

**As a** Super Administrator  
**I want** secure login and JWT token issuance  
**So that** users can access APIs safely based on identity

## Context

### Persona Reference
**Super Administrator** - platform-wide governance and secure access control  
[Full persona details](../personas.md#super-administrator)

## Acceptance Criteria

### AC1: Valid credential login returns JWT
- **Given** a registered user with valid credentials
- **When** login is requested
- **Then** the system returns access token, expiration, and role claims

### AC2: Invalid credential login is rejected
- **Given** invalid credentials
- **When** login is requested
- **Then** the system returns 401 Unauthorized

### AC3: Refresh token renews session
- **Given** a valid refresh token
- **When** refresh endpoint is called
- **Then** a new access token is issued and old token lifecycle is handled securely

