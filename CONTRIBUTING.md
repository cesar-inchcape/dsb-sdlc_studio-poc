# Contributing to DSB-PoC

Thank you for your interest in contributing to the Digital Service Booking (DSB) Proof of Concept! This document provides guidelines and instructions for contributing to the project.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Commit Messages](#commit-messages)
- [Pull Request Process](#pull-request-process)

---

## Code of Conduct

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

---

## Getting Started

### Prerequisites

- .NET 8.0 SDK or higher
- Visual Studio Code or Visual Studio 2022
- Git
- SQL Server (for production) or in-memory database (for development)

### Development Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/cesar-inchcape/dsb-sdlc_studio-poc.git
   cd dsb-sdlc_studio-poc
   ```

2. **Navigate to the main project:**
   ```bash
   cd "DSB - DXP+"
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore Login.Api
   dotnet restore Login.Api.Tests
   ```

4. **Build the solution:**
   ```bash
   dotnet build Login.Api
   ```

5. **Run tests to verify setup:**
   ```bash
   dotnet test Login.Api.Tests
   ```

6. **Start the API server:**
   ```bash
   dotnet run --project Login.Api
   ```

The API will be available at `https://localhost:5001` and Swagger documentation at `https://localhost:5001/swagger`

---

## Development Workflow

### Branch Naming Convention

Create branches following this pattern:

- **Feature branches:** `feature/FEATURE-NAME` (e.g., `feature/user-management`)
- **Bug fix branches:** `bugfix/BUG-NAME` (e.g., `bugfix/login-validation`)
- **Documentation branches:** `docs/DOC-NAME` (e.g., `docs/api-reference`)
- **Chore branches:** `chore/TASK-NAME` (e.g., `chore/upgrade-dependencies`)

### Feature Development Process

1. **Create a new branch from `main`:**
   ```bash
   git checkout main
   git pull origin main
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the coding standards below

3. **Write/update tests** for your changes (see Testing Requirements)

4. **Run all tests locally:**
   ```bash
   dotnet test Login.Api.Tests
   ```

5. **Commit your changes** with clear messages (see Commit Messages)

6. **Push your branch:**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Open a Pull Request** on GitHub

### Working on Existing Features

If adding to an existing feature (e.g., extending User Management):

1. Follow the same vertical slice structure
2. Place your code in the appropriate `Features/{FeatureName}/` folder
3. Follow the same naming conventions:
   - Commands: `{Action}{Resource}Command.cs`
   - Queries: `Get{Resource}Query.cs`
   - Handlers: `{Operation}{Resource}Handler.cs`
   - Validators: `{Resource}Validator.cs`
   - Endpoints: `{Resource}Endpoints.cs`

---

## Coding Standards

### C# Code Style

This project follows the [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

- **Naming:**
  - Classes, methods, properties: PascalCase
  - Private fields, local variables: camelCase
  - Constants: UPPER_SNAKE_CASE
  
- **Spacing:**
  - 4 spaces for indentation
  - Blank line between methods/properties
  - Blank line between logical groups

- **Braces:**
  - Opening brace on same line
  - All blocks must have braces (even single-line)

### CQRS Pattern

All features must follow the CQRS pattern:

```
Features/
└── FeatureName/
    ├── {Action}{Resource}Command.cs       (Define what to do)
    ├── {Action}{Resource}CommandHandler.cs (Do it)
    ├── {Action}{Resource}Validator.cs      (Validate input)
    ├── Get{Resource}Query.cs               (Read-only operations)
    ├── Get{Resource}QueryHandler.cs
    └── {Resource}Endpoints.cs              (HTTP routing)
```

### Documentation

- All public classes and methods must have XML documentation comments
- Document parameters, return values, and exceptions
- Include usage examples in complex methods

Example:
```csharp
/// <summary>
/// Creates a new user with the specified email and role.
/// </summary>
/// <param name="email">The user's email address (must be unique)</param>
/// <param name="firstName">The user's first name</param>
/// <param name="lastName">The user's last name</param>
/// <returns>The created user with assigned ID</returns>
/// <exception cref="InvalidOperationException">Thrown if email already exists</exception>
public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
{
    // implementation
}
```

---

## Testing Requirements

### Unit Test Coverage

- **Minimum coverage:** 80%
- **Test organization:** One test class per handler
- **Naming convention:** `{ClassName}Tests.cs`
- **Test method names:** `{Method}_{Scenario}_{ExpectedResult}`

Example:
```csharp
[Fact]
public async Task CreateUser_WithValidEmail_ReturnsNewUser()
{
    // Arrange
    var command = new CreateUserCommand("user@example.com", "John", "Doe");
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.Email.Should().Be("user@example.com");
}
```

### Integration Test Coverage

- One integration test per endpoint
- Test both success and failure paths
- Include authorization/authentication tests

### Running Tests

```bash
# All tests
dotnet test Login.Api.Tests

# Specific test class
dotnet test Login.Api.Tests --filter "CreateUserCommandTests"

# With coverage
dotnet test Login.Api.Tests --collect:"XPlat Code Coverage"
```

---

## Commit Messages

Use clear, descriptive commit messages following this format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring without feature changes
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance, dependencies, etc.

### Scope

The feature or area affected: `auth`, `users`, `workshops`, `advisors`, `schedules`, etc.

### Subject

- Start with lowercase letter
- Use imperative mood ("add" not "adds" or "added")
- No period at the end
- Max 50 characters

### Body

- Explain what and why, not how
- Wrap at 72 characters
- Separate from subject with blank line

### Footer

Link to related issues:
```
Closes #123
Fixes #456
```

### Examples

```
feat(users): add email verification for new users

Implement email verification workflow for newly created users.
Includes sending verification email and endpoint to confirm email.

Closes #45
```

```
fix(workshops): correct schedule overlap validation

Fixed bug where workshop schedules weren't properly validating
overlapping time blocks on same day.

Fixes #89
```

---

## Pull Request Process

### Before Submitting a PR

1. **Update from main:**
   ```bash
   git fetch origin
   git rebase origin/main
   ```

2. **Run all tests:**
   ```bash
   dotnet test Login.Api.Tests
   ```

3. **Build without warnings:**
   ```bash
   dotnet build Login.Api /p:TreatWarningsAsErrors=true
   ```

4. **Update documentation** if you changed functionality

### PR Title Format

Follow the same format as commit messages:
```
feat(scope): brief description
```

### PR Description Template

```markdown
## Description
Brief description of the changes and why they were made.

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Closes #123

## Changes Made
- Detailed bullet point of changes
- Another change
- And another

## Testing
- [ ] Added unit tests
- [ ] Added integration tests
- [ ] All tests passing (175/175)
- [ ] Manual testing completed

## Screenshots/Examples
(If applicable, include before/after or examples)

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Tests pass locally
```

### Review Process

- At least one review required before merge
- Address all comments before final approval
- Squash commits into logical chunks if needed
- Merge using "Squash and merge" for cleaner history

---

## Questions or Need Help?

- Review the [DEVELOPMENT.md](DEVELOPMENT.md) guide
- Check [QUICK_START.md](DSB%20-%20DXP%2B/QUICK_START.md) for setup help
- Review [IMPLEMENTATION_GUIDE.md](DSB%20-%20DXP%2B/IMPLEMENTATION_GUIDE.md) for architecture details
- Open an issue on GitHub for bugs or suggestions

---

## License

By contributing to this project, you agree that your contributions will be licensed under the project's existing license.

---

**Happy contributing!** 🚀
