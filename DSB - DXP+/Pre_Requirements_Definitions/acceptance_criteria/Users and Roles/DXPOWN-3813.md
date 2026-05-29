3PD Digital Service Booking - 3PD Autum + Americar | Distributor Administrator Restricted to Own Scope

AS a Distributor administrator
I WANT to access only the workshops, services, and customer data belonging to my distributor
SO THAT I do not see or affect other distributors’ operations.

Acceptance Criteria
AC1 – Scope Restriction
GIVEN a Distributor admin logs in
WHEN accessing any data
THEN the system MUST restrict visibility to:

Workshops belonging to their distributor

Services enabled for their distributor

Advisors assigned to their distributor

Data created under their scope

AC2 – Customer Data Limited to Their Own Appointments
GIVEN a Distributor admin accesses customer information
WHEN data is shown
THEN the system MUST ONLY expose customer records that:

Have booked appointments in their workshops, or

Have created vehicle or profile information tied to their workshops

AC3 – No Access to Other Distributors
GIVEN a Distributor admin views reports or dashboards
WHEN the data loads
THEN no information from other distributors, brands, or dealers not assigned to them MUST be shown.