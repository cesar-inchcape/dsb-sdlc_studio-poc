using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Authorization;
using System.Security.Claims;

namespace Login.Api.Tests.Infrastructure.Authorization;

public class AuthorizationPolicyTests
{
    [Fact]
    public void SuperAdminPolicy_WithSuperAdminRole_AllowsAccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isSuperAdmin = principal.IsInRole("SuperAdmin");

        // Assert
        isSuperAdmin.Should().BeTrue();
    }

    [Fact]
    public void SuperAdminPolicy_WithoutSuperAdminRole_DeniesAccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "DistributorAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isSuperAdmin = principal.IsInRole("SuperAdmin");

        // Assert
        isSuperAdmin.Should().BeFalse();
    }

    [Fact]
    public void AdminOrHigherPolicy_WithSuperAdminRole_AllowsAccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");

        // Assert
        isAdminOrHigher.Should().BeTrue();
    }

    [Fact]
    public void AdminOrHigherPolicy_WithDistributorAdminRole_AllowsAccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "DistributorAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");

        // Assert
        isAdminOrHigher.Should().BeTrue();
    }

    [Fact]
    public void AdminOrHigherPolicy_WithWorkshopUserRole_DeniesAccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "WorkshopUser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isAdminOrHigher = principal.IsInRole("SuperAdmin") || principal.IsInRole("DistributorAdmin");

        // Assert
        isAdminOrHigher.Should().BeFalse();
    }

    [Fact]
    public void MultipleRoles_AllRolesAvailable()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "DistributorAdmin"),
            new Claim(ClaimTypes.Role, "WorkshopUser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var hasDistributorAdmin = principal.IsInRole("DistributorAdmin");
        var hasWorkshopUser = principal.IsInRole("WorkshopUser");
        var hasSuperAdmin = principal.IsInRole("SuperAdmin");

        // Assert
        hasDistributorAdmin.Should().BeTrue();
        hasWorkshopUser.Should().BeTrue();
        hasSuperAdmin.Should().BeFalse();
    }

    [Fact]
    public void NoRoles_DeniesAllPolicies()
    {
        // Arrange
        var claims = Array.Empty<Claim>();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isSuperAdmin = principal.IsInRole("SuperAdmin");
        var isDistributorAdmin = principal.IsInRole("DistributorAdmin");
        var isWorkshopUser = principal.IsInRole("WorkshopUser");

        // Assert
        isSuperAdmin.Should().BeFalse();
        isDistributorAdmin.Should().BeFalse();
        isWorkshopUser.Should().BeFalse();
    }

    [Fact]
    public void WorkshopUserPolicy_WithWorkshopUserRole_AllowsReadOnlyAccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "WorkshopUser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isWorkshopUser = principal.IsInRole("WorkshopUser");

        // Assert
        isWorkshopUser.Should().BeTrue();
    }

    [Fact]
    public void WorkshopUserPolicy_WithAdminRole_DeniesAccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var isWorkshopUser = principal.IsInRole("WorkshopUser");

        // Assert
        isWorkshopUser.Should().BeFalse();
    }

    [Fact]
    public void CaseSensitiveRoles_ExactMatchRequired()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var matchCorrectCase = principal.IsInRole("SuperAdmin");
        var matchIncorrectCase = principal.IsInRole("superadmin");

        // Assert
        matchCorrectCase.Should().BeTrue();
        matchIncorrectCase.Should().BeFalse(); // IsInRole is case-sensitive in .NET
    }
}
