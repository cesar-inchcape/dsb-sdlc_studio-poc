using Xunit;
using FluentAssertions;
using Login.Api.Infrastructure.Security;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Login.Api.Tests.Infrastructure.Security;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;

    public JwtTokenGeneratorTests()
    {
        // Setup configuration with test JWT settings
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "ThisIsAVerySecretKeyForTestingPurposesOnly12345678"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiryMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _tokenGenerator = new JwtTokenGenerator(_configuration);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUserData_ReturnsJwtString()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "admin@test.com";
        var roles = new[] { "SuperAdmin" };

        // Act
        var token = _tokenGenerator.GenerateAccessToken(userId, email, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts separated by dots
    }

    [Fact]
    public void GenerateAccessToken_TokenContainsExpectedClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "admin@test.com";
        var roles = new[] { "SuperAdmin", "Distributor" };

        // Act
        var token = _tokenGenerator.GenerateAccessToken(userId, email, roles);
        var principal = _tokenGenerator.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(email);
        
        var roleClaims = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        roleClaims.Should().Contain("SuperAdmin");
        roleClaims.Should().Contain("Distributor");
    }

    [Fact]
    public void GenerateAccessToken_TokenExpirationIsSetCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@test.com";
        var roles = new[] { "User" };

        // Act
        var token = _tokenGenerator.GenerateAccessToken(userId, email, roles);
        var principal = _tokenGenerator.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var expClaim = principal!.FindFirst("exp")?.Value;
        expClaim.Should().NotBeNullOrEmpty();
        
        var expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim!));
        var now = DateTimeOffset.UtcNow;
        var expectedExpiry = now.AddMinutes(60);
        
        expiryTime.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@test.com";
        var roles = new[] { "User" };
        var token = _tokenGenerator.GenerateAccessToken(userId, email, roles);

        // Act
        var principal = _tokenGenerator.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange - Create configuration with very short expiry
        var shortExpiryConfig = new Dictionary<string, string>
        {
            {"Jwt:Secret", "ThisIsAVerySecretKeyForTestingPurposesOnly12345678"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiryMinutes", "0"} // Immediate expiry
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(shortExpiryConfig!)
            .Build();

        var generator = new JwtTokenGenerator(config);
        var token = generator.GenerateAccessToken(Guid.NewGuid(), "test@test.com", new[] { "User" });

        System.Threading.Thread.Sleep(1000); // Wait for token to expire

        // Act
        var principal = generator.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithInvalidSignature_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@test.com";
        var roles = new[] { "User" };
        var token = _tokenGenerator.GenerateAccessToken(userId, email, roles);

        // Tamper with the token signature
        var parts = token.Split('.');
        var tamperedToken = $"{parts[0]}.{parts[1]}.InvalidSignature";

        // Act
        var principal = _tokenGenerator.ValidateToken(tamperedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "this.is.not.a.valid.jwt";

        // Act
        var principal = _tokenGenerator.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsSecureRandomString()
    {
        // Act
        var refreshToken1 = _tokenGenerator.GenerateRefreshToken();
        var refreshToken2 = _tokenGenerator.GenerateRefreshToken();

        // Assert
        refreshToken1.Should().NotBeNullOrEmpty();
        refreshToken2.Should().NotBeNullOrEmpty();
        refreshToken1.Should().NotBe(refreshToken2); // Should be unique
        refreshToken1.Length.Should().BeGreaterThanOrEqualTo(32); // Should be reasonably long
    }
}
