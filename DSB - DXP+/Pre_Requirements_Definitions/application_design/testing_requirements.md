# Automated Testing Requirements Guide (Unit & API Tests)

This document establishes the mandatory guidelines and standards for implementing automated testing across all application modules, covering both Frontend and Backend layers.

All reference technical documentation regarding the technology stack and architectural design is located in the root `/architecture` folder.

---

## 1. Global Testing Strategy

A comprehensive testing coverage is required for every developed module. No feature or functionality will be considered complete or ready for production (*Definition of Done*) without its corresponding automated test suite:

1. **Unit Tests:** Focused on isolating and verifying the behavior of functions, components, utilities, and pure business logic independently.
2. **API Tests (Integration):** Focused on validating the endpoints exposed by the Backend, ensuring data contract consistency (JSON), HTTP status codes, authentication, and integration workflows.

---

## 2. Architectural Alignment & Technology Stack

The development of test cases must strictly align with the application's architectural definitions. Before proceeding with test coding, it is mandatory to review the reference material located in:

* **`/architecture/backend/`**: Contains the technical specifications of the server-side architecture (e.g., clean architecture, layering, dependency injection) and the chosen tech stack.
* **`/architecture/frontend/`**: Contains the diagrams, conventions, design patterns, and libraries defined for the user interface.

*Note: Since the base documentation inside the `/architecture` folder is written in English, the testing codebase (function names, variables, and case descriptions like `it('should...')`) must remain in **English** to guarantee consistency with the core architecture.*

---

## 3. Requirements per Module

For each new or modified module within the repository, the following criteria must be fulfilled:

### A. Backend
* **Unit Tests:**
  * Coverage of core business logic (Services / Use Cases).
  * Mocking of the data access layer (Repositories / Database) and external dependencies.
  * Validation of exception handling, edge cases, and alternative workflows.
* **API Tests:**
  * Functional integration tests running against controller or routing layers.
  * Proper verification of HTTP response status codes (`200 OK`, `201 Created`, `400 Bad Request`, `401 Unauthorized`, `404 Not Found`, etc.).
  * Strict validation of the response payload schema and formats.

### B. Frontend
* **Unit Tests:**
  * Component UI testing (basic rendering, event handling, and internal state changes).
  * Validation of global state management elements (actions, reducers, selectors) and custom hooks.
  * Simulation of API service calls utilizing interceptors or mocking utility libraries.

---

## 4. Execution & Quality Gate

* **Automation:** Tests must be executable locally via a single standard command (e.g., `npm run test`, `mvn test`, `pytest`, depending on the specific stack defined in `/architecture`).
* **Continuous Integration (CI):** Every *Pull Request* will automatically trigger the entire test suite. The failure of a single test case will block code merge and deployment pipelines.
