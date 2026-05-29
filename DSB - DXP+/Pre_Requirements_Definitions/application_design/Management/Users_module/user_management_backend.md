# prompt_create_users_crud.md

## Context & Objective
You are an expert backend developer specialized in .NET 10, Entity Framework Core, and SQL Server.
Your objective is to generate the complete code for a **CRUD feature slice** for managing **Users** within the **Management.Api** project.

This feature must follow **Vertical Slice Architecture**, keeping all handlers, validators, DTOs, and controllers grouped inside the feature directory: `src/Services/Management.Api/Features/Users/`.

---

## 1. Target Domain & Schema Context

Map operations to the following tables defined in our schema:

### Table: Users
* `Id` uniqueidentifier (Primary Key)
* `CountryId` uniqueidentifier (Foreign Key -> Countries.Id, nullable if applicable, otherwise mandatory)
* `AdvisorId` uniqueidentifier (Foreign Key -> Advisors.Id, nullable)
* `Email` varchar(150)
* `PasswordHash` varchar(500)
* `IsActive` bit
* `CreatedAt` datetime2

### Table: UserRoles (For Role assignment during Create/Update)
* `UserId` uniqueidentifier
* `RoleId` uniqueidentifier
* `CreatedAt` datetime2

---

## 2. Architectural Guidelines & Requirements

* **Vertical Slice Design:** Do not use traditional technical layers. Group each operation (Create, Read/Get, Update, Delete) into its own sub-folder or file inside `Features/Users/`.
* **Asynchronous Processing:** Every single database pathway, mapping, or operation must utilize asynchronous signatures (`Async`/`Await`).
* **Data Access Strategy:**
    * **EF Core (Basic CRUD):** Use standard EF Core DbContext tracking for Simple Gets, Updates, and Inserts.
    * **Stored Procedures via EF Core (Complex/Custom Queries):** Provide the SQL T-SQL script (`CREATE OR ALTER PROCEDURE`) and the corresponding execution snippet via EF Core (`FromSqlRaw` or `ExecuteSqlRawAsync`) for:
        1. **Paginated Filtered List:** A stored procedure to retrieve users filtered by Email or Status.
        2. **User Creation Transaction:** A stored procedure or transactional EF block that safely ensures the User is created, the password is encrypted, and roles are assigned simultaneously.
* **DTOs Isolation:** Define strict, immutable `public record` structures for each `RequestDto` and `ResponseDto`. No domain models or database entity structures must pass beyond the API Controllers/Endpoints.
* **Validation:** Write validation layers using **FluentValidation** for incoming request payloads before handling any business logic.
* **Security Baselines:** * Passwords must not be stored in plaintext. Generate a mock utility call or service abstraction to simulate **BCrypt** or **Argon2** hashing.
    * Prevent SQL injection by strictly parameterizing inputs via EF Core.

---

## 3. Required Deliverables

Please generate the following file components with complete production-ready implementations:

### Part 1: SQL Infrastructure Scripts
* `database/stored_procedures/management/sp_GetUsers_Paginated.sql`: T-SQL script using `CREATE OR ALTER PROCEDURE` managing inputs for pagination (`@PageSize`, `@PageNumber`) and filters (`@Email`, `@IsActive`).

### Part 2: C# Feature Slices (`src/Services/Management.Api/Features/Users/`)

Generate the specific clean architecture components using MediatR (or minimal endpoint handlers if MediatR isn't preferred):

1.  **CreateUser Slice (`Features/Users/CreateUser/`)**
    * `CreateUserCommand.cs` & `CreateUserHandler.cs`
    * `CreateUserValidator.cs` (FluentValidation confirming email format, required fields).
    * `CreateUserDtos.cs` (`CreateUserRequest`, `CreateUserResponse`).
2.  **GetUserById & GetUsersList Slices (`Features/Users/GetUsers/`)**
    * Queries and Handlers executing the compiled paginated Stored Procedure through EF Core.
    * `UserResponseDto.cs` mapping out the view model.
3.  **UpdateUser Slice (`Features/Users/UpdateUser/`)**
    * Command and Handler for modifying baseline parameters (`IsActive`, `CountryId`, `AdvisorId`).
4.  **DeleteUser Slice (`Features/Users/DeleteUser/`)**
    * Command performing logical soft deletion or direct record dropping depending on state dependencies.

### Part 3: API Routing Integration
* `UsersController.cs` or Minimal API Endpoints extension mapping `POST`, `GET`, `PUT`, and `DELETE` requests directly to their respective feature handlers using standard RESTful naming conventions.

---

## 4. Output Generation Rule
Provide clean, readable, well-commented code following modern C# 12 / .NET 10 conventions. Avoid summaries or placeholder omissions (`// TODO: implement later`). Ensure all entity properties line up perfectly with the structural database types.