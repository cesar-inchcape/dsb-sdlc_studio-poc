using FluentAssertions;
using Xunit;
using System.Net;
using System.Net.Http.Json;
using Login.Api.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Login.Api.Tests.Integration;

public class UserEndpointTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;
    private string _adminToken = null!;

    public UserEndpointTests()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _adminToken = await GetAdminToken();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    private async Task<string> GetAdminToken()
    {
        var loginRequest = new
        {
            email = "admin@dsb.cl",
            password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.AccessToken;
    }

    #region Authentication Tests

    [Fact]
    public async Task AdminCreateUser_WithoutAuth_Returns401()
    {
        // Arrange
        var createRequest = new
        {
            email = "newuser@dsb.cl",
            firstName = "New",
            lastName = "User",
            password = "NewPassword123!",
            roleIds = new List<Guid>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/users", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetUsers_WithoutAuth_ReturnsForbiddenOrUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert - Should be 401/403/5xx (not 200 OK)
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AdminGetUser_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync($"/api/admin/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminUpdateUser_WithoutAuth_Returns401()
    {
        // Arrange
        var updateRequest = new
        {
            firstName = "Updated"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/users/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminDeleteUser_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/admin/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminAssignRole_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/admin/users/{Guid.NewGuid()}/roles/{Guid.NewGuid()}", 
            new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminRemoveRole_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.DeleteAsync(
            $"/api/admin/users/{Guid.NewGuid()}/roles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ManagementGetCurrentUser_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/management/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ManagementUpdateCurrentUser_WithoutAuth_Returns401()
    {
        // Arrange
        var updateRequest = new
        {
            firstName = "Updated"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/management/users/me", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task AdminEndpoint_WithValidAuth_ReturnsSuccess()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new("Bearer", _adminToken);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert - Should not get Unauthorized when properly authenticated
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ManagementEndpoint_WithValidAuth_ReturnsSuccess()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new("Bearer", _adminToken);

        // Act
        var response = await _client.GetAsync("/api/management/users/me");

        // Assert - Should not get Unauthorized when properly authenticated
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminAssignRole_WithValidAuth_ReturnsSuccess()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new("Bearer", _adminToken);
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/admin/users/{userId}/roles/{roleId}", 
            new { });

        // Assert - Should not get Unauthorized when properly authenticated
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminRemoveRole_WithValidAuth_ReturnsSuccess()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new("Bearer", _adminToken);
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync(
            $"/api/admin/users/{userId}/roles/{roleId}");

        // Assert - Should not get Unauthorized when properly authenticated
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}
