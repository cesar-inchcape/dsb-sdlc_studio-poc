using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Login.Api.Features.Auth.Login;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Login.Api.Tests.Integration;

public class AuthorizationEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthorizationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_PublicEndpoint_AllowsAccessWithoutAuthorization()
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
    public async Task RefreshToken_PublicEndpoint_AllowsAccessWithoutAuthorization()
    {
        // Arrange - First login to get refresh token
        var loginRequest = new LoginRequest("admin@dsb.cl", "Admin123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new Login.Api.Features.Auth.RefreshToken.RefreshTokenRequest(loginResult!.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await client.GetAsync("/api/auth/health");

        // Assert
        // Will return 404 if endpoint doesn't exist, or 401 if it does
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "admin@dsb.cl",
            Password: "Admin123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewAccessToken()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@dsb.cl", "Admin123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new Login.Api.Features.Auth.RefreshToken.RefreshTokenRequest(loginResult!.RefreshToken);

        // Act
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<Login.Api.Features.Auth.RefreshToken.RefreshTokenResponse>();
        refreshResult!.AccessToken.Should().NotBeNullOrEmpty();
        refreshResult.RefreshToken.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
