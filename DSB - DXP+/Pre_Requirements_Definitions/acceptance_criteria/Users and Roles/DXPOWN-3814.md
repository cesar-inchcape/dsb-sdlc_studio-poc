3PD Digital Service Booking - 3PD Autum + Americar | Workshop User With Read-Only Service Permissions


AS a workshop user
I WANT read‑only access to service configurations
SO THAT I can consult my workshop’s setup without modifying services.

Acceptance Criteria
AC1 – Read‑Only Service Configuration
GIVEN the workshop user accesses the service configuration module
WHEN viewing services
THEN the user MUST NOT be able to:

Edit

Create

Delete services

AC2 – Operational Permissions
GIVEN the workshop user accesses operational features
WHEN interacting with schedules
THEN the user MUST be allowed to:

View appointments

Reassign advisors

Block time slots

Mark show/no show

Create an appointment or delete one

Export reports or view dashboards

AC3 – No Access to High‑Level Configurations
GIVEN the workshop user profile
WHEN navigating the system
THEN the user MUST NOT see:

Brand-level settings

Distributor-level management

Other workshops from other regions or brands