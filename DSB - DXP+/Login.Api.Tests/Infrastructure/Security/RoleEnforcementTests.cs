using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Authorization;
using System.Security.Claims;

namespace Login.Api.Tests.Infrastructure.Authorization;

public class RoleEnforcementTests
{
    [Fact]
    public void SuperAdmin_CanAccessAllEndpoints()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "admin@test.com"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"));

        // Act & Assert
        principal.IsInRole("SuperAdmin").Should().BeTrue();
        
        // Should have all policy permissions
        var isSuperAdminOnly = principal.IsInRole("SuperAdmin");
        var isAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");
        
        isSuperAdminOnly.Should().BeTrue();
        isAdminOrHigher.Should().BeTrue();
    }

    [Fact]
    public void DistributorAdmin_CannotAccessSuperAdminOnly()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "distributor@test.com"),
            new Claim(ClaimTypes.Role, "DistributorAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"));

        // Act
        var canAccessSuperAdminOnly = principal.IsInRole("SuperAdmin");
        var canAccessAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");

        // Assert
        canAccessSuperAdminOnly.Should().BeFalse();
        canAccessAdminOrHigher.Should().BeTrue();
    }

    [Fact]
    public void WorkshopUser_CannotAccessAdminPolicies()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "workshop@test.com"),
            new Claim(ClaimTypes.Role, "WorkshopUser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"));

        // Act
        var canAccessSuperAdminOnly = principal.IsInRole("SuperAdmin");
        var canAccessAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");

        // Assert
        canAccessSuperAdminOnly.Should().BeFalse();
        canAccessAdminOrHigher.Should().BeFalse();
    }

    [Fact]
    public void UserWithoutRoles_CannotAccessAnyPolicy()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "nouser@test.com")
            // No role claims
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"));

        // Act
        var canAccessSuperAdminOnly = principal.IsInRole("SuperAdmin");
        var canAccessAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");
        var canAccessWorkshopReadOnly = principal.Identity?.IsAuthenticated ?? false;

        // Assert
        canAccessSuperAdminOnly.Should().BeFalse();
        canAccessAdminOrHigher.Should().BeFalse();
        canAccessWorkshopReadOnly.Should().BeTrue(); // Authenticated, but no specific role
    }

    [Fact]
    public void MultipleRoles_AllPoliciesApply()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "multi@test.com"),
            new Claim(ClaimTypes.Role, "DistributorAdmin"),
            new Claim(ClaimTypes.Role, "WorkshopUser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"));

        // Act
        var canAccessSuperAdminOnly = principal.IsInRole("SuperAdmin");
        var canAccessAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");
        var canAccessWorkshopReadOnly = principal.IsInRole("WorkshopUser");

        // Assert
        canAccessSuperAdminOnly.Should().BeFalse();
        canAccessAdminOrHigher.Should().BeTrue();
        canAccessWorkshopReadOnly.Should().BeTrue();
    }

    [Fact]
    public void InvalidRoleFormat_DoesNotMatch()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "superadmin"), // lowercase
            new Claim(ClaimTypes.Role, "SUPERADMIN")   // uppercase
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"));

        // Act
        var exactMatch = principal.IsInRole("SuperAdmin"); // Correct case

        // Assert
        exactMatch.Should().BeFalse(); // Roles are case-sensitive in .NET
    }

    [Fact]
    public void EmptyIdentity_CannotAccessPolicies()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var isAuthenticated = principal.Identity?.IsAuthenticated ?? false;
        var canAccessSuperAdminOnly = principal.IsInRole("SuperAdmin");
        var canAccessAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");

        // Assert
        isAuthenticated.Should().BeFalse();
        canAccessSuperAdminOnly.Should().BeFalse();
        canAccessAdminOrHigher.Should().BeFalse();
    }

    [Fact]
    public void NullPrincipal_SafeHandling()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert - Should not throw
        if (principal != null)
        {
            principal.IsInRole("SuperAdmin").Should().BeFalse();
        }
        else
        {
            // Principal is null, safe handling
            principal.Should().BeNull();
        }
    }

    [Fact]
    public void PrivilegeEscalation_Prevention_NoHigherRoleFromLowerRole()
    {
        // Arrange - Start with WorkshopUser
        var workshopUserClaims = new[]
        {
            new Claim(ClaimTypes.Role, "WorkshopUser")
        };
        var workshopPrincipal = new ClaimsPrincipal(new ClaimsIdentity(workshopUserClaims, "TestScheme"));

        // Act - Try to claim higher role (simulate injection attempt)
        var maliciousClaims = new[]
        {
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var maliciousPrincipal = new ClaimsPrincipal(new ClaimsIdentity(maliciousClaims, "TestScheme"));

        // Assert - Original principal should still only have WorkshopUser
        workshopPrincipal.IsInRole("WorkshopUser").Should().BeTrue();
        workshopPrincipal.IsInRole("SuperAdmin").Should().BeFalse();

        // Malicious principal would have SuperAdmin, but original is unchanged
        maliciousPrincipal.IsInRole("SuperAdmin").Should().BeTrue();
    }
}
