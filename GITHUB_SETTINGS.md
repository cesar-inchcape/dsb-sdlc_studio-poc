# GitHub Repository Settings

Recommended GitHub settings for the DSB-PoC project to ensure code quality and team collaboration.

## Repository Protection

### Main Branch Protection Rules

To protect the `main` branch and ensure code quality:

1. **Go to:** Settings → Branches → Add rule
2. **Branch name pattern:** `main`
3. **Configure:**

#### Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- ✅ Require status checks to pass before merging
  - `continuous-integration/windows` (or your CI provider)
  - `continuous-integration/tests`
  - All other required checks

#### Require code reviews before merging
- ✅ Require pull request reviews before merging
- **Number of reviewers:** 1
- ✅ Require review from Code Owners
- ✅ Dismiss stale pull request approvals when new commits are pushed
- ✅ Require approval of the most recent reviewers

#### Require conversation resolution before merging
- ✅ Require all conversations on code to be resolved

#### Require signed commits
- ✅ Require signed commits

#### Require commit history to be straight
- ✅ Require pull requests to be merged with a squash or rebase

## Branch Naming Convention

Enforce branch naming convention:
- `feature/*` — New features
- `bugfix/*` — Bug fixes
- `docs/*` — Documentation
- `chore/*` — Maintenance tasks
- `hotfix/*` — Emergency fixes

## PR and Issue Labels

### Suggested Labels

#### Type
- `enhancement` — New feature request
- `bug` — Something isn't working
- `documentation` — Improvements or additions to documentation
- `refactor` — Code refactoring without functionality changes
- `test` — Improvements to tests
- `chore` — Maintenance tasks, dependency updates

#### Priority
- `priority: critical` — Must be addressed immediately
- `priority: high` — Important for current iteration
- `priority: medium` — Important but not urgent
- `priority: low` — Nice to have, future consideration

#### Status
- `status: blocked` — Blocked by another issue/PR
- `status: in-progress` — Currently being worked on
- `status: review` — Under review
- `status: ready` — Ready for implementation

#### Phase
- `phase-1` — Authentication and core features
- `phase-2` — Booking functionality
- `phase-3` — Reports and analytics
- `phase-4` — Frontend

#### Component
- `component: auth` — Authentication
- `component: users` — User management
- `component: workshops` — Workshop management
- `component: advisors` — Advisor management
- `component: schedules` — Schedule management
- `component: tests` — Testing infrastructure
- `component: docs` — Documentation
- `component: ci-cd` — CI/CD pipeline

### Auto-labeling Rules

Configure GitHub to auto-label issues:
- PRs from dependabot → `chore`, `dependencies`
- Issues with templates → `bug` or `enhancement` (based on template)

## Automated Workflows

### Suggested GitHub Actions

Create `.github/workflows/` files for:

1. **CI/CD Pipeline:** `.github/workflows/dotnet.yml`
2. **Code Quality:** Automated linting and formatting checks
3. **Dependency Updates:** Dependabot configuration
4. **Release:** Automated release notes generation

### Example: .github/workflows/dotnet.yml

```yaml
name: .NET Build and Test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore "DSB - DXP+/Login.Api/Login.Api.csproj"
    
    - name: Build
      run: dotnet build "DSB - DXP+/Login.Api/Login.Api.csproj" --no-restore
    
    - name: Test
      run: dotnet test "DSB - DXP+/Login.Api.Tests/Login.Api.Tests.csproj" --no-build --verbosity normal
```

## Dependabot Configuration

### .github/dependabot.yml

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/DSB - DXP+/Login.Api"
    schedule:
      interval: "weekly"
      day: "monday"
      time: "03:00"
    pull-request-branch-name:
      separator: "/"
    reviewers:
      - "cesar-inchcape"
    allow:
      - dependency-type: "production"
      - dependency-type: "development"
```

## Repository General Settings

### About
- **Description:** Digital Service Booking (DSB) - Proof of Concept API built with ASP.NET Core 8.0
- **Homepage:** https://github.com/cesar-inchcape/dsb-sdlc_studio-poc
- **Topics:** `dotnet`, `aspnetcore`, `api`, `cqrs`, `microservices`, `proof-of-concept`

### Access & Security
- **Private/Public:** Public
- **Allow discussions:** Yes
- **Wikis:** Enable (for internal documentation)
- **Issues:** Enable
- **Projects:** Enable (for project management)

### Collaborator Access

Recommended roles:
- **Maintainers:** Full access
- **Contributors:** Push access after code review
- **External Reviewers:** Pull request review only

## Code Owners

### .github/CODEOWNERS

```
# GitHub handles must have write access to the repo

# All changes
* @cesar-inchcape

# API changes
DSB - DXP+/Login.Api/** @cesar-inchcape

# Test changes
DSB - DXP+/Login.Api.Tests/** @cesar-inchcape

# Documentation
*.md @cesar-inchcape
.github/** @cesar-inchcape
```

## Release Management

### Release Process

1. **Version:** Use semantic versioning (MAJOR.MINOR.PATCH)
2. **Tags:** Create Git tags for releases (e.g., `v1.0.0`)
3. **Changelog:** Maintain CHANGELOG.md
4. **Release Notes:** Generate from commit messages and PRs

### Release Checklist

- [ ] All PRs merged and tested
- [ ] Version bumped in `.csproj` files
- [ ] CHANGELOG.md updated
- [ ] Git tag created: `v1.x.x`
- [ ] GitHub Release created with notes
- [ ] Deployment to staging verified
- [ ] Deployment to production completed

## Milestone & Project Management

### Milestones

Create milestones for:
- **Phase 1 - Complete** (past)
- **Phase 2 - Booking.Api** (current)
- **Phase 3 - Management.Api** (future)
- **Phase 4 - Frontend** (future)

### Projects (Kanban Boards)

Suggested columns:
- 📋 **Backlog** — Not started
- 🎯 **Ready** — Ready for development
- 🚀 **In Progress** — Currently being worked on
- 🔍 **In Review** — Under peer review
- ✅ **Done** — Completed and merged

## Notifications & Alerts

### Team Notifications
- Pull request reviews required
- CI/CD failures
- Dependency updates
- Security alerts

### Suggested Slack Integration (if using Slack)
- PR approvals needed
- CI failures
- New releases
- Security updates

## Security & Compliance

### Security Settings

- ✅ Enable dependency alerts
- ✅ Enable security alerts for vulnerable dependencies
- ✅ Require signed commits for main branch
- ✅ Enable branch protection for `main`

### Secret Scanning

- Repository secrets: Use GitHub Secrets for sensitive data
- No credentials in code: Use `.gitignore` properly

## Repository Badges (README.md)

Add to README.md for visibility:

```markdown
[![Tests](https://github.com/cesar-inchcape/dsb-sdlc_studio-poc/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cesar-inchcape/dsb-sdlc_studio-poc/actions)
[![Code Style](https://img.shields.io/badge/code%20style-C%23-239120?logo=csharp)](CONTRIBUTING.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
```

## Monitoring & Analytics

### Check These Insights

- **Traffic:** Repository views and clones
- **Forks:** Track project adoption
- **Network:** Visualize branching and merging
- **Pulse:** Activity overview
- **Community:** Health metrics

## Next Steps

1. **Configure branch protection** for `main`
2. **Create labels** for issues and PRs
3. **Set up GitHub Actions** for CI/CD
4. **Add CODEOWNERS** file
5. **Enable Dependabot** for security
6. **Configure release automation** (optional)
7. **Set up project board** for issue tracking

---

For specific guidance on each step, refer to [GitHub Docs](https://docs.github.com) or ask the project maintainer.
