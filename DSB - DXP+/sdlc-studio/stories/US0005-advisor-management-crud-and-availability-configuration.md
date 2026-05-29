# US0005: Advisor Management CRUD and Availability Configuration

> **Status:** Draft
> **Epic:** [EP0002: Management API Core Entities](../epics/EP0002-management-api-core-entities.md)
> **Owner:** TBD
> **Reviewer:** TBD
> **Created:** 2026-05-29

## User Story

**As a** Distributor Administrator  
**I want** manage advisor profiles and availability  
**So that** workshop capacity is configured accurately

## Context

### Persona Reference
**Distributor Administrator** - operational control in assigned scope  
[Full persona details](../personas.md#distributor-administrator)

## Acceptance Criteria

### AC1: Advisor CRUD is supported
- **Given** valid advisor payloads
- **When** CRUD operations are executed
- **Then** advisor profile data is stored and retrievable with validations

### AC2: Advisor schedules and blocks are configurable
- **Given** an advisor assigned to a workshop
- **When** weekly schedules or ad-hoc blocks are configured
- **Then** availability configuration is persisted and queryable

### AC3: Deletion constraints are respected
- **Given** an advisor with active bookings
- **When** deletion is requested
- **Then** the system warns and prevents unsafe removal per business rule

