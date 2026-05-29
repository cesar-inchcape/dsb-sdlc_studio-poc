using Microsoft.AspNetCore.Authorization;

namespace Login.Api.Infrastructure.Authorization;

/// <summary>
/// Authorization policies for role-based access control.
/// Policies define which roles are allowed to access specific resources or operations.
/// </summary>
public static class AuthorizationPolicies
{
    // Policy names
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string AdminOrHigher = "AdminOrHigher";
    public const string WorkshopReadOnly = "WorkshopReadOnly";

    /// <summary>
    /// Registers all authorization policies with the authorization service.
    /// Call this in Program.cs during service configuration.
    /// </summary>
    /// <example>
    /// builder.Services.AddAuthorization(options =>
    /// {
    ///     AuthorizationPolicies.RegisterPolicies(options);
    /// });
    /// </example>
    public static void RegisterPolicies(AuthorizationOptions options)
    {
        // SuperAdmin policy: Only SuperAdmin role can access
        options.AddPolicy(SuperAdminOnly, policy =>
            policy.RequireRole("SuperAdmin"));

        // AdminOrHigher policy: SuperAdmin or DistributorAdmin can access
        options.AddPolicy(AdminOrHigher, policy =>
            policy.RequireRole("SuperAdmin", "DistributorAdmin"));

        // WorkshopReadOnly policy: All authenticated users (read-only enforcement at endpoint level)
        // Write operations should be blocked by separate middleware or attribute
        options.AddPolicy(WorkshopReadOnly, policy =>
            policy.RequireAuthenticatedUser());
    }
}
