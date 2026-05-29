using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Login.Api.Features.Auth.Login;
using Login.Api.Features.Auth.RefreshToken;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Login.Api.Tests.Integration;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200OK()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "admin@dsb.cl",
            Password: "Admin123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessTokenAndRefreshToken()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "admin@dsb.cl",
            Password: "Admin123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
        loginResponse.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Email.Should().Be("admin@dsb.cl");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_Returns401Unauthorized()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "nonexistent@dsb.cl",
            Password: "SomePassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401Unauthorized()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "admin@dsb.cl",
            Password: "WrongPassword!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNullEmail_Returns400BadRequest()
    {
        // Arrange
        var request = new LoginRequest(
            Email: null!,
            Password: "Admin123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_Returns400BadRequest()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "admin@dsb.cl",
            Password: ""
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_Returns200OK()
    {
        // Arrange - First login to get refresh token
        var loginRequest = new LoginRequest("admin@dsb.cl", "Admin123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new RefreshTokenRequest(loginResult!.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange - First login to get refresh token
        var loginRequest = new LoginRequest("admin@dsb.cl", "Admin123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new RefreshTokenRequest(loginResult!.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        var refreshResult = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResult.Should().NotBeNull();
        refreshResult!.AccessToken.Should().NotBeNullOrEmpty();
        refreshResult.RefreshToken.Should().NotBeNullOrEmpty();
        refreshResult.AccessToken.Should().NotBe(loginResult.AccessToken); // New token
        refreshResult.RefreshToken.Should().NotBe(loginResult.RefreshToken); // New refresh token
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_Returns401Unauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest("invalid-refresh-token-12345");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_Returns401Unauthorized()
    {
        // Arrange - Login and use refresh token once
        var loginRequest = new LoginRequest("admin@dsb.cl", "Admin123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new RefreshTokenRequest(loginResult!.RefreshToken);
        
        // Use the refresh token (this revokes it)
        await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Act - Try to use the same refresh token again (should fail)
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
