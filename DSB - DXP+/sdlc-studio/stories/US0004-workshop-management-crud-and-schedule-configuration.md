# US0004: Workshop Management CRUD and Schedule Configuration

> **Status:** Draft
> **Epic:** [EP0002: Management API Core Entities](../epics/EP0002-management-api-core-entities.md)
> **Owner:** TBD
> **Reviewer:** TBD
> **Created:** 2026-05-29

## User Story

**As a** Distributor Administrator  
**I want** manage workshops and operating schedules  
**So that** each dealer location is configured correctly

## Context

### Persona Reference
**Distributor Administrator** - manages operations in assigned scope  
[Full persona details](../personas.md#distributor-administrator)

## Acceptance Criteria

### AC1: Workshop CRUD is supported
- **Given** valid workshop data
- **When** CRUD operations are performed
- **Then** workshop records are maintained with required validations

### AC2: Workshop schedule and blocks can be configured
- **Given** an existing workshop
- **When** weekly schedules or ad-hoc blocks are updated
- **Then** the system persists and exposes schedule configuration correctly

### AC3: Role/scope restrictions apply to workshop changes
- **Given** a user outside allowed scope
- **When** attempting workshop changes
- **Then** the API returns 403 Forbidden

