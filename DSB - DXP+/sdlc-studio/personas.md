# User Personas

Personas generated from `sdlc-studio/prd.md` for the DSB - DXP+ project.

---

## End Customer

**Role:** Vehicle owner booking service appointments  
**Technical Proficiency:** Intermediate  
**Primary Goal:** Book a workshop appointment quickly and confidently

### Background
Customer of one of the supported brands in Chile who needs routine maintenance or repair services and prefers digital self-service over phone calls.

### Needs & Motivations
- Clear booking flow with available dates and times
- Immediate confirmation after scheduling
- Trust that the selected workshop and advisor are correct

### Pain Points
- No visibility of real availability
- Long call-center wait times
- Confusing service options

### Typical Tasks
- Select brand/workshop/service
- Pick available date and time
- Confirm and review booking details

### Quote
> "If I can book in two minutes from my phone, I'll use it every time."

---

## Distributor Administrator

**Role:** Distributor operations administrator  
**Technical Proficiency:** Advanced  
**Primary Goal:** Operate users, workshops and advisors within assigned scope

### Background
Internal business operator responsible for one distributor/brand/region with accountability for workshop operations and service quality.

### Needs & Motivations
- Strict scope-based access controls
- Fast CRUD operations for workshops and advisors
- Reliable operational visibility

### Pain Points
- Access to out-of-scope data creates risk
- Manual processes for schedule updates
- Inconsistent permissions across teams

### Typical Tasks
- Create/edit users with role assignments
- Manage workshop data and schedules
- Maintain advisor availability

### Quote
> "Give me full control of my scope, but nothing outside it."

---

## Super Administrator

**Role:** Platform administrator  
**Technical Proficiency:** Expert  
**Primary Goal:** Govern configuration and access across all dealers and brands

### Background
Central platform owner accountable for governance, access model, and platform-wide configuration quality.

### Needs & Motivations
- Unrestricted administrative access
- Consistent RBAC model
- Auditability and secure authentication

### Pain Points
- Fragmented configuration processes
- Permission drift across services
- Security risks from weak auth controls

### Typical Tasks
- Configure roles and permissions
- Manage multi-brand platform settings
- Validate security and access behavior

### Quote
> "I need one secure control plane for the full platform."

---

## Workshop User

**Role:** Workshop staff user  
**Technical Proficiency:** Basic to Intermediate  
**Primary Goal:** View service configuration without accidental modification

### Background
Daily workshop operator who needs accurate service information but should not alter master configuration.

### Needs & Motivations
- Fast read-only access
- Clear UI permissions
- Safe guardrails against accidental edits

### Pain Points
- Overly complex admin screens
- Permission confusion
- Risk of unintended changes

### Typical Tasks
- Review services and availability context
- Validate appointment details for customers
- Coordinate with admins on required changes

### Quote
> "Show me what I need, and hide what I shouldn't touch."

